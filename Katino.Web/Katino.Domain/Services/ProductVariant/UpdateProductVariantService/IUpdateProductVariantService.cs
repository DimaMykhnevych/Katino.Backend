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
        string sourceOrderTtnSnapshot = null,
        bool isPendingPhysicalArrival = false);

    // Splits a freed-up quantity into its real and still-in-transit (phantom) parts and redistributes each
    // separately, real part first: the real part gets funneled into the most urgent unmet need, and whatever
    // that leaves in stock plus the phantom part is redistributed next, tagged as still pending arrival.
    // isPendingPhysicalArrival must never be inferred from current DB state inside HandleProductVariantQuantityChange
    // itself - by the time this runs the originating pending-return row may already be consumed
    // (see SewingProductionReportService.ApplyIncomingReturnSewedAsync), so callers must pass it explicitly.
    Task HandleProductVariantQuantityChangeSplit(
        Guid productVariantId,
        int originalQuantityInStock,
        int realDelta,
        int phantomDelta,
        Guid? orderIdToSkipFromProcessing,
        ProductVariantQuantityChangeReason reason,
        string sourceOrderTtnSnapshot = null);
}
