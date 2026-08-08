using Katino.Domain.Entities;
using Katino.Domain.Enums;
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

    public async Task<ProductVariant> GetWithProductColorAndSize(Guid id)
    {
        return await context.ProductVariants
            .Include(x => x.Product)
            .Include(x => x.Color)
            .Include(x => x.Size)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<ProductVariant>> GetManyWithProductAndCollectionAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        return await context.ProductVariants
            .Where(pv => idList.Contains(pv.Id))
            .Include(pv => pv.Product)
                .ThenInclude(p => p.ProductCollections)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsActiveBySizeAndColorAsync(Guid productId, Guid sizeId, Guid colorId)
    {
        return await context.ProductVariants.AnyAsync(v =>
            v.ProductId == productId &&
            v.SizeId == sizeId &&
            v.ColorId == colorId &&
            v.DeletedAt == null &&
            v.Status != ProductStatus.Discontinued);
    }
}
