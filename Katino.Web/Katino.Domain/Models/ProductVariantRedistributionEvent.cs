using Katino.Domain.Enums;

namespace Katino.Domain.Models;

public class ProductVariantRedistributionEvent
{
    public Guid ProductVariantId { get; set; }

    // Why this quantity moved, kept purely for audit/display - see IsPendingPhysicalArrival for the logic flag.
    public ProductVariantQuantityChangeReason Reason { get; set; }
    public Guid? SourceOrderId { get; set; }
    public Guid? SourceOrderItemId { get; set; }
    public string SourceOrderTtnSnapshot { get; set; }

    // True when every line in this event is still physically in transit (not yet produced/received),
    // so the target orders need the PendingIncomingReturn tag rather than being treated as truly ready.
    public bool IsPendingPhysicalArrival { get; set; }
    public List<ProductVariantRedistributionLine> Lines { get; set; } = [];
}

public class ProductVariantRedistributionLine
{
    public Guid? TargetOrderId { get; set; }
    public Guid? TargetOrderItemId { get; set; }
    public int Quantity { get; set; }
}
