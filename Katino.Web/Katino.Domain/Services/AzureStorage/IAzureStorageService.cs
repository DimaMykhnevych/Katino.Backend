using Microsoft.AspNetCore.Http;

namespace Katino.Domain.Services.AzureStorage;

public interface IAzureStorageService
{
    Task<List<string>> UploadPhotosAsync(Guid productVariantId, IFormFileCollection files);

    Task<string> CopyPhotoAsync(string sourceUrl, Guid targetVariantId);

    Task DeletePhotoAsync(Guid productVariantId, string photoUrl);
}
