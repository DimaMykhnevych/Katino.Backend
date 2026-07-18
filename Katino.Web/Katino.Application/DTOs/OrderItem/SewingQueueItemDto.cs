using Katino.Application.DTOs.ProductVariant;

namespace Katino.Application.DTOs.OrderItem;

public class SewingQueueItemDto
{
    public Guid ProductVariantId { get; set; }
    public int QuantityToProduce { get; set; }
    public bool IsCustomTailoring { get; set; }
    public bool IsIncomingReturn { get; set; }
    public DateTime? SendUntil { get; set; }
    public string Comment { get; set; }
    public Guid? OrderItemId { get; set; }

    public ProductVariantDto ProductVariant { get; set; } = default!;
}
