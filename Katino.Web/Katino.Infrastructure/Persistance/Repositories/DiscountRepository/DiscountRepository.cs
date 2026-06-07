using Katino.Domain.Entities;
using Katino.Domain.Repositories.DiscountRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.DiscountRepository;

public class DiscountRepository : Repository<Discount>, IDiscountRepository
{
    public DiscountRepository(KatinoDbContext context) : base(context) { }

    public async Task<List<Discount>> GetAllWithDetailsAsync()
    {
        return await context.Discounts
            .Include(d => d.DiscountProducts).ThenInclude(dp => dp.Product)
            .Include(d => d.DiscountCollections).ThenInclude(dc => dc.Collection)
            .Include(d => d.BundleProducts).ThenInclude(bp => bp.Product)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Discount> GetWithDetailsAsync(Guid id)
    {
        return await context.Discounts
            .Include(d => d.DiscountProducts).ThenInclude(dp => dp.Product)
            .Include(d => d.DiscountCollections).ThenInclude(dc => dc.Collection)
            .Include(d => d.BundleProducts).ThenInclude(bp => bp.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Discount>> GetActiveWithDetailsAsync()
    {
        var now = DateTime.UtcNow;
        return await context.Discounts
            .Where(d => d.IsActive
                && (d.StartDate == null || d.StartDate <= now)
                && (d.EndDate == null || d.EndDate >= now))
            .Include(d => d.DiscountProducts)
            .Include(d => d.DiscountCollections)
            .Include(d => d.BundleProducts)
            .AsNoTracking()
            .ToListAsync();
    }
}
