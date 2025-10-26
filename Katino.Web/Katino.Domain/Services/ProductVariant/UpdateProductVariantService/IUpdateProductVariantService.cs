using Katino.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;

public interface IUpdateProductVariantService
{
    Task<bool> UpdateProductVariantAsync(ProductVariant productVariant, IFormFileCollection newPhotos, List<Guid> photoIdsToDelete);
}
