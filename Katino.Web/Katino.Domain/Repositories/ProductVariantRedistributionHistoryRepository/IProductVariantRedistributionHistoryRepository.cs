using Katino.Domain.Entities;
using Katino.Domain.Models;

namespace Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;

public interface IProductVariantRedistributionHistoryRepository : IRepository<ProductVariantRedistributionHistory>
{
    Task<IEnumerable<ProductVariantRedistributionHistory>> GetByOrderIdAsync(Guid orderId);

    // Rejected-order returns still in transit: the target order still carries the PendingIncomingReturn tag,
    // meaning it hasn't been Packed yet, so the redistributed item hasn't physically arrived.
    // Grouped by TargetOrderItemId, so an item covered by several partial pending returns still shows as one line.
    Task<List<PendingIncomingReturnSummary>> GetPendingIncomingReturnsAsync(Guid? sewerId = null, CancellationToken ct = default);

    // Sum of Quantity - QuantityResolved across every open (IsPendingPhysicalArrival) row targeting this order item.
    Task<int> GetPendingReturnRemainingAsync(Guid orderItemId, CancellationToken ct = default);

    // Resolves up to `quantity` of open pending-return rows for this order item, oldest first, incrementing
    // QuantityResolved. Used both when a replacement gets sewn and when another order takes over the pending
    // coverage (e.g. urgent reallocation, order deletion/edit) - one place computing the FIFO consumption instead
    // of duplicating it at each call site. Returns how much was actually consumed (may be less than requested).
    Task<int> ConsumePendingReturnAsync(Guid orderItemId, int quantity, CancellationToken ct = default);

    Task<bool> HasOpenPendingReturnsForOrderAsync(Guid orderId, CancellationToken ct = default);
}
