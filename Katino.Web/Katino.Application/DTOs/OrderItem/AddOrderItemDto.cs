namespace Katino.Application.DTOs.OrderItem;

public class AddOrderItemDto
{
    public bool IsCustomTailoring { get; set; }
    public string Comment { get; set; }
    public int Quantity { get; set; }
    public Guid ProductVariantId { get; set; }
}
