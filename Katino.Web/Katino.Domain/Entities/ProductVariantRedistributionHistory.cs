using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class ProductVariantRedistributionHistory
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariantQuantityChangeReason Reason { get; set; }
    public int Quantity { get; set; }

    // The order that freed up this quantity (rejected/deleted/donor), if any. Null for Sewing/ManualEdit.
    public Guid? SourceOrderId { get; set; }

    // The specific donor order item, only known for UrgentReallocation (rejection/deletion return whole orders, not single items).
    public Guid? SourceOrderItemId { get; set; }

    // Snapshot, because SourceOrderId can point to an order that no longer exists by the time this is read.
    public string SourceOrderTtnSnapshot { get; set; }

    // The order the freed quantity was redistributed to. Null means it just went into stock.
    public Guid? TargetOrderId { get; set; }
    public Guid? TargetOrderItemId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public ProductVariant ProductVariant { get; set; }
    public Order SourceOrder { get; set; }
    public Order TargetOrder { get; set; }
}
