using Katino.Domain.Entities;

namespace Katino.Domain.Models;

// One row per order item, aggregating every open (IsPendingPhysicalArrival) redistribution history
// row that still targets it - so a sewer sees a single line even if the item was covered by several
// partial pending returns.
public class PendingIncomingReturnSummary
{
    public Guid ProductVariantId { get; init; }
    public ProductVariant ProductVariant { get; init; } = default!;
    public Guid TargetOrderId { get; init; }
    public Guid TargetOrderItemId { get; init; }
    public DateTime SendUntilDate { get; init; }
    public int RemainingQuantity { get; init; }
}
