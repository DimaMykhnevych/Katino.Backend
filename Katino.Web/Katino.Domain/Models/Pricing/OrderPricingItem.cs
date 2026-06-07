namespace Katino.Domain.Models.Pricing;

public class OrderPricingItem
{
    public Guid ProductVariantId { get; set; }
    public Guid ProductId { get; set; }
    public IReadOnlyList<Guid> CollectionIds { get; set; } = [];
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public bool IsCustomTailoring { get; set; }
}
