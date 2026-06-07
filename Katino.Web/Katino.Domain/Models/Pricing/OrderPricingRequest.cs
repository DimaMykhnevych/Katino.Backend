namespace Katino.Domain.Models.Pricing;

public class OrderPricingRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public bool IsCustomTailoring { get; set; }
}
