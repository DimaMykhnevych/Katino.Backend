using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.ProductVariantRepository;

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<ProductVariant> GetWithMeasurements(Guid id);
}
