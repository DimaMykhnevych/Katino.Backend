using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Domain.Services.ProductVariantRedistributionN.ProductVariantRedistributionRecorder;

namespace Katino.Infrastructure.Persistance.Services.ProductVariantRedistributionN;

public class ProductVariantRedistributionRecorder : IProductVariantRedistributionRecorder
{
    private readonly IProductVariantRedistributionHistoryRepository _historyRepository;
    private readonly IOrderTagRepository _orderTagRepository;

    public ProductVariantRedistributionRecorder(
        IProductVariantRedistributionHistoryRepository historyRepository,
        IOrderTagRepository orderTagRepository)
    {
        _historyRepository = historyRepository;
        _orderTagRepository = orderTagRepository;
    }

    public async Task RecordAsync(ProductVariantRedistributionEvent redistributionEvent)
    {
        if (redistributionEvent.Lines.Count == 0)
        {
            return;
        }

        foreach (var line in redistributionEvent.Lines)
        {
            await _historyRepository.Insert(new ProductVariantRedistributionHistory
            {
                Id = Guid.NewGuid(),
                ProductVariantId = redistributionEvent.ProductVariantId,
                Reason = redistributionEvent.Reason,
                Quantity = line.Quantity,
                SourceOrderId = redistributionEvent.SourceOrderId,
                SourceOrderItemId = redistributionEvent.SourceOrderItemId,
                SourceOrderTtnSnapshot = redistributionEvent.SourceOrderTtnSnapshot,
                TargetOrderId = line.TargetOrderId,
                TargetOrderItemId = line.TargetOrderItemId,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        await _historyRepository.Save();

        // The item from a rejected order is physically still in transit back to the warehouse,
        // so orders it gets redistributed to need a visible marker that it's not in stock yet.
        if (redistributionEvent.Reason == ProductVariantQuantityChangeReason.OrderRejected)
        {
            var targetOrderIds = redistributionEvent.Lines
                .Where(l => l.TargetOrderId.HasValue)
                .Select(l => l.TargetOrderId.Value)
                .Distinct();

            if (targetOrderIds.Any())
            {
                var tag = await _orderTagRepository.GetOrCreateByTypeAsync(OrderTagType.PendingIncomingReturn, canBeDeleted: true);

                foreach (var targetOrderId in targetOrderIds)
                {
                    if (!await _orderTagRepository.IsTagAttachedToOrderAsync(targetOrderId, tag.Id))
                    {
                        await _orderTagRepository.AttachTagToOrderAsync(targetOrderId, tag.Id);
                    }
                }

                await _orderTagRepository.Save();
            }
        }
    }
}
