using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;

public interface IUpdateProductVariantService
{
    Task<bool> UpdateProductVariantAsync(ProductVariant productVariant, IFormFileCollection newPhotos, List<Guid> photoIdsToDelete, List<Guid> sewerIds);
    Task HandleProductVariantQuantityChange(
        Guid productVariantId,
        int newQuantity,
        Guid? orderIdToSkipFromProcessing = null,
        ProductVariantQuantityChangeReason reason = ProductVariantQuantityChangeReason.ManualEdit,
        string sourceOrderTtnSnapshot = null);
}
