using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Katino.Domain.Services.AzureStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Katino.Domain.Constants;

namespace Katino.Infrastructure.Persistance.Services.AzureStorage;

public class AzureStorageService : IAzureStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger _logger;

    public AzureStorageService(
        BlobServiceClient blobServiceClient,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration[ConfigurationKeys.StoragePhotoContainerName];
        _logger = loggerFactory?.CreateLogger(nameof(AzureStorageService));
    }

    public async Task<List<string>> UploadPhotosAsync(Guid productVariantId, IFormFileCollection files)
    {
        var photoUrls = new List<string>();
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                try
                {
                    var fileName = $"{productVariantId}/{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var blobClient = containerClient.GetBlobClient(fileName);

                    using var stream = file.OpenReadStream();
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = file.ContentType
                        }
                    });

                    var photoUrl = blobClient.Uri.ToString();
                    photoUrls.Add(photoUrl);

                    _logger.LogInformation($"Photo uploaded successfully: {photoUrl}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error uploading photo: {file.FileName}");
                    throw;
                }
            }
        }

        return photoUrls;
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
            _logger.LogInformation($"Photo deleted successfully: {photoUrl}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting photo: {photoUrl}");
            throw;
        }
    }
}
