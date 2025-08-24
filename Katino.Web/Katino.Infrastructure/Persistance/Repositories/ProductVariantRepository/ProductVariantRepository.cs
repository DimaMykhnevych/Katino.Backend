using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.ProductVariantRepository;

public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(KatinoDbContext context) : base(context)
    {
    }
}
