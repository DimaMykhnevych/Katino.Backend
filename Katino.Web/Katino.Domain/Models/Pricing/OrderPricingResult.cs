using Katino.Domain.Enums;

namespace Katino.Domain.Models.Pricing;

public class OrderPricingResult
{
    public decimal BaseTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalTotal { get; set; }
    public List<ItemPricingResult> ItemResults { get; set; } = [];
}

public class ItemPricingResult
{
    public Guid ProductVariantId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalLineTotal { get; set; }
    public Guid? DiscountId { get; set; }
    public DiscountType? AppliedDiscountType { get; set; }
}
