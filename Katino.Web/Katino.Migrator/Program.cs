using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Text;

// ──────────────────────────────────────────────
// Config
// ──────────────────────────────────────────────

Console.OutputEncoding = Encoding.UTF8;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var dbConnectionString = config.GetConnectionString("DefaultConnection")!;
var storageConnString = config["AzureStorage:ConnectionString"]!;
var containerName = config["AzureStorage:ContainerName"]!;
var backupContainerName = config["AzureStorage:BackupContainerName"]!;
var webpQuality = int.Parse(config["Migration:WebpQuality"] ?? "80");
var dryRun = bool.Parse(config["Migration:DryRun"] ?? "false");

if (dryRun)
    Console.WriteLine("⚠️  DRY RUN mode — no changes will be applied\n");

// ──────────────────────────────────────────────
// Azure clients
// ──────────────────────────────────────────────
var blobServiceClient = new BlobServiceClient(storageConnString);
var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
var backupContClient = blobServiceClient.GetBlobContainerClient(backupContainerName);

//await ContainerStats();
//return;

async Task ContainerStats()
{
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

    int totalCount = 0;
    long totalBytes = 0;

    await foreach (var blob in containerClient.GetBlobsAsync())
    {
        totalCount++;
        totalBytes += blob.Properties.ContentLength ?? 0;
    }

    Console.WriteLine($"Blobs: {totalCount}");
    Console.WriteLine($"Total size: {totalBytes / 1024 / 1024.0:F1} MB");
}

await backupContClient.CreateIfNotExistsAsync(PublicAccessType.None);


// ──────────────────────────────────────────────
// DB context
// ──────────────────────────────────────────────
var optionsBuilder = new DbContextOptionsBuilder<MigrationDbContext>();

var serverVersion = ServerVersion.AutoDetect(dbConnectionString);
optionsBuilder.UseMySql(dbConnectionString, serverVersion);

await using var db = new MigrationDbContext(optionsBuilder.Options);

// ──────────────────────────────────────────────
// Load all db files
// ──────────────────────────────────────────────
var photos = await db.ProductPhotos
    .Where(p => !p.PhotoUrl.EndsWith(".webp"))
    .OrderBy(p => p.ProductVariantId)
    .ToListAsync();

Console.WriteLine($"Found {photos.Count} photos for conversion\n");

if (photos.Count == 0)
{
    Console.WriteLine("Nothing to convert. Ending execution.");
    return;
}

// ──────────────────────────────────────────────
// Stats
// ──────────────────────────────────────────────
int successCount = 0;
int skipCount = 0;
int errorCount = 0;
long totalSavedBytes = 0;

var encoder = new WebpEncoder
{
    Quality = webpQuality,
    Method = WebpEncodingMethod.BestQuality,
    FileFormat = WebpFileFormatType.Lossy
};

// ──────────────────────────────────────────────
// Photo processing
// ──────────────────────────────────────────────
var processedUrls = new Dictionary<string, string>();
foreach (var photo in photos)
{
    Console.Write($"[{successCount + skipCount + errorCount + 1}/{photos.Count}] {photo.PhotoUrl}  →  ");
    if (processedUrls.TryGetValue(photo.PhotoUrl, out var cachedNewUrl))
    {
        Console.WriteLine($"♻️  already converted previously → {cachedNewUrl}");
        if (!dryRun)
            photo.PhotoUrl = cachedNewUrl;
        successCount++;
        continue;
    }

    try
    {
        var uri = new Uri(photo.PhotoUrl);
        var blobName = string.Join("/", uri.Segments.Skip(2).Select(s => s.Trim('/'))).Trim('/');
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            Console.WriteLine("⚠️  blob not found, skipping...");
            skipCount++;
            continue;
        }

        // ── 1. Downloading original photo ──
        using var originalStream = new MemoryStream();
        await blobClient.DownloadToAsync(originalStream);
        var originalSize = originalStream.Length;
        originalStream.Position = 0;

        // ── 2. Creating backup ──
        if (!dryRun)
        {
            var backupClient = backupContClient.GetBlobClient(blobName);
            originalStream.Position = 0;

            if (!await backupClient.ExistsAsync())
            {
                var blobProps = await blobClient.GetPropertiesAsync();
                await backupClient.UploadAsync(originalStream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = blobProps.Value.ContentType
                    }
                });
            }
            originalStream.Position = 0;
        }

        // ── 3. WebP conversion ──
        using var webpStream = new MemoryStream();
        using (var image = await Image.LoadAsync(originalStream))
        {
            image.Mutate(x => x.AutoOrient());
            await image.SaveAsync(webpStream, encoder);
        }
        var webpSize = webpStream.Length;
        webpStream.Position = 0;

        // ── 4. New blob name ──
        var newBlobName = Path.ChangeExtension(blobName, ".webp");

        // ── 5. Create new URL ──
        var newBlobClient = containerClient.GetBlobClient(newBlobName);
        var newPhotoUrl = newBlobClient.Uri.ToString();

        processedUrls[photo.PhotoUrl] = newPhotoUrl;

        if (!dryRun)
        {
            // ── 6. Uploading webp ──
            await newBlobClient.UploadAsync(webpStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "image/webp" }
            });

            // ── 7. Updating URL in DB ──
            photo.PhotoUrl = newPhotoUrl;

            // ── 8. Delete old blob ──
            await blobClient.DeleteIfExistsAsync();
        }

        var savedKb = (originalSize - webpSize) / 1024.0;
        var savedPct = (1.0 - (double)webpSize / originalSize) * 100;
        totalSavedBytes += Math.Max(0, originalSize - webpSize);

        Console.WriteLine($"✅  {originalSize / 1024}KB → {webpSize / 1024}KB  (−{savedPct:F0}%)");
        successCount++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌  Error: {ex.Message}");
        errorCount++;
    }
}

// ──────────────────────────────────────────────
// Saving DB
// ──────────────────────────────────────────────
if (!dryRun && successCount > 0)
{
    Console.WriteLine($"\nSaving DB updates...");
    await db.SaveChangesAsync();
    Console.WriteLine("DB updated.");
}

// ──────────────────────────────────────────────
// Results
// ──────────────────────────────────────────────
Console.WriteLine($"""

╔══════════════════════════════╗
  Migration completed
  ✅  Success:     {successCount}
  ⚠️  Skipped:     {skipCount}
  ❌  Errors:      {errorCount}
  💾  Saved space: {totalSavedBytes / 1024 / 1024.0:F1} MB
╚══════════════════════════════╝
""");

// ──────────────────────────────────────────────
// DbContext
// ──────────────────────────────────────────────
public class ProductPhoto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class MigrationDbContext : DbContext
{
    public MigrationDbContext(DbContextOptions<MigrationDbContext> options) : base(options) { }

    public DbSet<ProductPhoto> ProductPhotos => Set<ProductPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductPhoto>().ToTable("ProductPhotos");
    }
}