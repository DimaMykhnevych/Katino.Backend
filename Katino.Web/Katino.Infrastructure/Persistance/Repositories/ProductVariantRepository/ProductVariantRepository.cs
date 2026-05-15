using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.ProductVariantRepository;

public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<ProductVariant> GetAsNoTracking(Guid id)
    {
        return await context.ProductVariants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProductVariant> GetWithMeasurements(Guid id)
    {
        return await context.ProductVariants
            .Include(x => x.Measurements)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProductVariant> GetWithMeasurementsAndSewers(Guid id)
    {
        return await context.ProductVariants
            .Include(x => x.Measurements)
            .Include(x => x.Sewers)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void DeleteSewer(ProductVariantSewer sewer)
    {
        context.ProductVariantSewers.Remove(sewer);
    }

    public async Task<ProductVariant> GetWithPhotos(Guid id)
    {
        return await context.ProductVariants
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProductVariant> GetWithProduct(Guid id)
    {
        return await context.ProductVariants
            .Include(x => x.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
