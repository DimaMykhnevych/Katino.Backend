using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.ProductVariantRepository;

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<ProductVariant> GetWithMeasurements(Guid id);
    Task<ProductVariant> GetWithMeasurementsAndSewers(Guid id);
    Task<ProductVariant> GetAsNoTracking(Guid id);
    Task<ProductVariant> GetWithPhotos(Guid id);
    void DeleteSewer(ProductVariantSewer sewer);
    Task<ProductVariant> GetWithProduct(Guid id);
    Task<ProductVariant> GetWithProductColorAndSize(Guid id);
}
