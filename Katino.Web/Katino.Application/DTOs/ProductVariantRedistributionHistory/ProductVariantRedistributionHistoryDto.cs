namespace Katino.Application.DTOs.ProductVariantRedistributionHistory;

public class ProductVariantRedistributionHistoryDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductVariantArticle { get; set; }
    public int Quantity { get; set; }
    public ProductVariantQuantityChangeReasonDto Reason { get; set; }

    public Guid? SourceOrderId { get; set; }
    public Guid? SourceOrderItemId { get; set; }
    public string SourceOrderTtn { get; set; }

    public Guid? TargetOrderId { get; set; }
    public Guid? TargetOrderItemId { get; set; }
    public string TargetOrderTtn { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
