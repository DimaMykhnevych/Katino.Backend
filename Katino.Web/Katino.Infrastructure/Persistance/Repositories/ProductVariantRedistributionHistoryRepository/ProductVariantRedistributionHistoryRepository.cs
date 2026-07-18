using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Models;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.ProductVariantRedistributionHistoryRepository;

public class ProductVariantRedistributionHistoryRepository : Repository<ProductVariantRedistributionHistory>, IProductVariantRedistributionHistoryRepository
{
    public ProductVariantRedistributionHistoryRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductVariantRedistributionHistory>> GetByOrderIdAsync(Guid orderId)
    {
        return await context.ProductVariantRedistributionHistory
            .Include(x => x.ProductVariant)
            .Include(x => x.TargetOrder)
            .Where(x => x.SourceOrderId == orderId || x.TargetOrderId == orderId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<List<PendingIncomingReturnSummary>> GetPendingIncomingReturnsAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        var rows = await context.ProductVariantRedistributionHistory
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x =>
                x.IsPendingPhysicalArrival &&
                x.TargetOrderItemId != null &&
                x.Quantity > x.QuantityResolved &&
                x.TargetOrder.OrderTags.Any(t => t.OrderTag.Type == OrderTagType.PendingIncomingReturn) &&
                (sewerId == null ||
                 x.ProductVariant.SewingQueueVisibility == SewingQueueVisibility.AllSewers ||
                 (x.ProductVariant.SewingQueueVisibility == SewingQueueVisibility.Specific &&
                  x.ProductVariant.Sewers.Any(s => s.SewerId == sewerId))))
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Product).ThenInclude(p => p.Category)
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Color)
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Size)
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Photos)
            .Include(x => x.TargetOrder)
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.TargetOrderItemId!.Value)
            .Select(g =>
            {
                var first = g.First();
                return new PendingIncomingReturnSummary
                {
                    ProductVariantId = first.ProductVariantId,
                    ProductVariant = first.ProductVariant,
                    TargetOrderId = first.TargetOrderId!.Value,
                    TargetOrderItemId = g.Key,
                    SendUntilDate = first.TargetOrder.SendUntilDate,
                    RemainingQuantity = g.Sum(x => x.Quantity - x.QuantityResolved)
                };
            })
            .ToList();
    }

    public async Task<int> GetPendingReturnRemainingAsync(Guid orderItemId, CancellationToken ct = default)
    {
        var remaining = await context.ProductVariantRedistributionHistory
            .Where(x =>
                x.IsPendingPhysicalArrival &&
                x.TargetOrderItemId == orderItemId &&
                x.Quantity > x.QuantityResolved)
            .SumAsync(x => (int?)(x.Quantity - x.QuantityResolved), ct);

        return remaining ?? 0;
    }

    public async Task<int> ConsumePendingReturnAsync(Guid orderItemId, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0)
        {
            return 0;
        }

        var openRows = await context.ProductVariantRedistributionHistory
            .Where(x =>
                x.IsPendingPhysicalArrival &&
                x.TargetOrderItemId == orderItemId &&
                x.Quantity > x.QuantityResolved)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        var remainingToConsume = quantity;
        foreach (var row in openRows)
        {
            if (remainingToConsume <= 0)
            {
                break;
            }

            var rowRemaining = row.Quantity - row.QuantityResolved;
            var toConsume = Math.Min(rowRemaining, remainingToConsume);
            row.QuantityResolved += toConsume;
            remainingToConsume -= toConsume;
        }

        var consumed = quantity - remainingToConsume;
        if (consumed > 0)
        {
            await context.SaveChangesAsync(ct);
        }

        return consumed;
    }

    public async Task<bool> HasOpenPendingReturnsForOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        return await context.ProductVariantRedistributionHistory
            .AnyAsync(x =>
                x.IsPendingPhysicalArrival &&
                x.TargetOrderId == orderId &&
                x.Quantity > x.QuantityResolved, ct);
    }
}
