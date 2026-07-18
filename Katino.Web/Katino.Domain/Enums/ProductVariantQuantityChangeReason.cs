namespace Katino.Domain.Enums;

public enum ProductVariantQuantityChangeReason
{
    ManualEdit,
    OrderRejected,
    OrderDeleted,
    OrderEdited,
    Sewing,
    UrgentReallocation,

    // A pending incoming return (OrderRejected) was covered by sewing a replacement instead of waiting for the physical return.
    ReturnCoveredBySewing
}
