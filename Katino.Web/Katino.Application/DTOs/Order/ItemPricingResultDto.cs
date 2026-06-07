namespace Katino.Application.DTOs.Order;

public class ItemPricingResultDto
{
    public Guid ProductVariantId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalLineTotal { get; set; }
    public Guid? DiscountId { get; set; }
    public DiscountTypeDto? AppliedDiscountType { get; set; }
}
