using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class ProductVariantRedistributionHistory
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }

    // Why this quantity moved, kept purely for audit/display. Whether the quantity is still physically
    // in transit is tracked independently via IsPendingPhysicalArrival, not derived from this.
    public ProductVariantQuantityChangeReason Reason { get; set; }
    public int Quantity { get; set; }

    // True while Quantity (minus QuantityResolved) is a virtual assignment - the item hasn't physically
    // arrived/been produced yet (e.g. a rejected order's return still in transit). Flips the row out of
    // "pending" once fully resolved by sewing a replacement or by being handed to another order.
    public bool IsPendingPhysicalArrival { get; set; }

    // How much of Quantity has been resolved: either sewn as a replacement, or reassigned/consumed by
    // another order taking over the pending coverage. Quantity itself is left untouched as the historical fact.
    public int QuantityResolved { get; set; }

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
