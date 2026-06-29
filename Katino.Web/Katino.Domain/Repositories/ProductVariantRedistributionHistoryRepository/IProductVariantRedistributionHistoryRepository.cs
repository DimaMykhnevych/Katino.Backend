using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;

public interface IProductVariantRedistributionHistoryRepository : IRepository<ProductVariantRedistributionHistory>
{
    Task<IEnumerable<ProductVariantRedistributionHistory>> GetByOrderIdAsync(Guid orderId);
}
