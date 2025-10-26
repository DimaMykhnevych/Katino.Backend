using Microsoft.AspNetCore.Http;

namespace Katino.Domain.Services.AzureStorage;

public interface IAzureStorageService
{
    Task<List<string>> UploadPhotosAsync(Guid productVariantId, IFormFileCollection files);

    Task DeletePhotoAsync(Guid productVariantId, string photoUrl);
}
