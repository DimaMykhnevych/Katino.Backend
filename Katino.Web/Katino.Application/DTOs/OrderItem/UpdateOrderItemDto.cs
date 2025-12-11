namespace Katino.Application.DTOs.OrderItem;

public class UpdateOrderItemDto
{
    public Guid? Id { get; set; }
    public bool IsCustomTailoring { get; set; }
    public string Comment { get; set; }
    public int Quantity { get; set; }

    public Guid ProductVariantId { get; set; }
}
