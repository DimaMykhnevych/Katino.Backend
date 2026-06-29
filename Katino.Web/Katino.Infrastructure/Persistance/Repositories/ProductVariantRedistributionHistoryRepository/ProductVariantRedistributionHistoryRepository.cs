using Katino.Domain.Entities;
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
}
