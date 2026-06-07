namespace Katino.Application.DTOs.Order;

public class OrderPricingRequestDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public bool IsCustomTailoring { get; set; }
}
