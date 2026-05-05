using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Katino.Domain.Constants;
using Katino.Domain.Services.AzureStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Katino.Infrastructure.Persistance.Services.AzureStorage;

public class AzureStorageService : IAzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly string _financeReportContainerName;
    private readonly ILogger _logger;

    private const int WebpQuality = 80;

    public AzureStorageService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration[ConfigurationKeys.StoragePhotoContainerName];
        _financeReportContainerName = configuration[ConfigurationKeys.FinanceReportContainerName];
        _logger = loggerFactory?.CreateLogger(nameof(AzureStorageService));
    }

    public async Task<List<string>> UploadPhotosAsync(Guid productVariantId, IFormFileCollection files)
    {
        var photoUrls = new List<string>();
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            try
            {
                var originalName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileName = $"{productVariantId}/{Guid.NewGuid()}_{originalName}.webp";
                var blobClient = containerClient.GetBlobClient(fileName);

                using var inputStream = file.OpenReadStream();
                using var outputStream = new MemoryStream();

                _logger.LogInformation($"Converting photo {fileName} to webp");
                await ConvertToWebpAsync(inputStream, outputStream);
                outputStream.Position = 0;

                await blobClient.UploadAsync(outputStream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/webp"
                    }
                });

                var photoUrl = blobClient.Uri.ToString();
                photoUrls.Add(photoUrl);

                _logger.LogInformation("Photo uploaded successfully: {PhotoUrl}", photoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo: {FileName}", file.FileName);
                throw;
            }
        }

        return photoUrls;
    }

    public async Task<string> CopyPhotoAsync(string sourceUrl, Guid targetVariantId)
    {
        try
        {
            var sourceUri = new Uri(sourceUrl);
            var sourceFileName = Path.GetFileName(sourceUri.LocalPath);
            var destFileName = $"{targetVariantId}/{Guid.NewGuid()}_{sourceFileName}";

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var destBlobClient = containerClient.GetBlobClient(destFileName);

            await destBlobClient.StartCopyFromUriAsync(sourceUri);

            _logger.LogInformation("Photo copied successfully from {SourceUrl} to {DestUrl}", sourceUrl, destBlobClient.Uri);

            return destBlobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying photo from {SourceUrl}", sourceUrl);
            throw;
        }
    }

    public async Task DeletePhotoAsync(Guid productVariantId, string photoUrl)
    {
        try
        {
            var uri = new Uri(photoUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var fullBlobName = $"{productVariantId}{Path.AltDirectorySeparatorChar}{blobName}";

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fullBlobName);

            await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Photo deleted successfully: {PhotoUrl}", photoUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo: {PhotoUrl}", photoUrl);
            throw;
        }
    }

    public async Task<(Stream Content, string ContentType)> GetFinanceReportScreenshotAsync(int year)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_financeReportContainerName);
        var blobClient = containerClient.GetBlobClient($"{year}.png");

        var response = await blobClient.DownloadStreamingAsync();
        return (response.Value.Content, response.Value.Details.ContentType ?? "image/png");
    }

    private static async Task ConvertToWebpAsync(Stream input, Stream output)
    {
        using var image = await Image.LoadAsync(input);

        var encoder = new WebpEncoder
        {
            Quality = WebpQuality,
            Method = WebpEncodingMethod.BestQuality,
            FileFormat = WebpFileFormatType.Lossy
        };

        image.Mutate(x => x.AutoOrient());
        await image.SaveAsync(output, encoder);
    }
}