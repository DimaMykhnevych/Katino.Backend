using Katino.Application.DTOs.ProductVariant;

namespace Katino.Application.DTOs.OrderItem;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public bool IsCustomTailoring { get; set; }
    public string Comment { get; set; }
    public int Quantity { get; set; }
    public OrderItemStatusDto OrderItemStatus { get; set; }
    public int QuantityToProduce { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid OrderId { get; set; }
    public ProductVariantForOrderDto ProductVariant { get; set; }
}
