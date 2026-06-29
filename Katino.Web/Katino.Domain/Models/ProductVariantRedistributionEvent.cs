using Katino.Domain.Enums;

namespace Katino.Domain.Models;

public class ProductVariantRedistributionEvent
{
    public Guid ProductVariantId { get; set; }
    public ProductVariantQuantityChangeReason Reason { get; set; }
    public Guid? SourceOrderId { get; set; }
    public Guid? SourceOrderItemId { get; set; }
    public string SourceOrderTtnSnapshot { get; set; }
    public List<ProductVariantRedistributionLine> Lines { get; set; } = [];
}

public class ProductVariantRedistributionLine
{
    public Guid? TargetOrderId { get; set; }
    public Guid? TargetOrderItemId { get; set; }
    public int Quantity { get; set; }
}
