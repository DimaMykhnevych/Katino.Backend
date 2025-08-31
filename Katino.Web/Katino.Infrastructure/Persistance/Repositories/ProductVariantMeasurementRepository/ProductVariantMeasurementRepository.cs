using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.ProductVariantMeasurementRepository;

public class ProductVariantMeasurementRepository : Repository<ProductVariantMeasurement>, IProductVariantMeasurementRepository
{
    public ProductVariantMeasurementRepository(KatinoDbContext context) : base(context)
    {
    }
}
