using Katino.Domain.Entities;
using Katino.Domain.Enums;
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

    public async Task<List<ProductVariantRedistributionHistory>> GetPendingIncomingReturnsAsync(CancellationToken ct = default)
    {
        return await context.ProductVariantRedistributionHistory
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x =>
                x.Reason == ProductVariantQuantityChangeReason.OrderRejected &&
                x.TargetOrderItemId != null &&
                x.TargetOrder.OrderTags.Any(t => t.OrderTag.Type == OrderTagType.PendingIncomingReturn))
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Product).ThenInclude(p => p.Category)
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Color)
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Size)
            .Include(x => x.ProductVariant).ThenInclude(pv => pv.Photos)
            .Include(x => x.TargetOrder)
            .ToListAsync(ct);
    }
}
