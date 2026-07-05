using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;

public interface IProductVariantRedistributionHistoryRepository : IRepository<ProductVariantRedistributionHistory>
{
    Task<IEnumerable<ProductVariantRedistributionHistory>> GetByOrderIdAsync(Guid orderId);

    // Rejected-order returns still in transit: the target order still carries the PendingIncomingReturn tag,
    // meaning it hasn't been Packed yet, so the redistributed item hasn't physically arrived.
    Task<List<ProductVariantRedistributionHistory>> GetPendingIncomingReturnsAsync(CancellationToken ct = default);
}
