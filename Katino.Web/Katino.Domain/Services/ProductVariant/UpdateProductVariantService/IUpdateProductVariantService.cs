using Katino.Domain.Entities;

namespace Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;

public interface IUpdateProductVariantService
{
    Task<bool> UpdateProductVariantAsync(ProductVariant productVariant);
}
