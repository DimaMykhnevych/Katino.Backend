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

    public async Task<ProductVariant> GetWithMeasurements(Guid id)
    {
        return await context.ProductVariants
            .Include(x => x.Measurements)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
