using Katino.Domain.Entities;

namespace Katino.Domain.Models;

public class SewingQueueItem
{
    public Guid ProductVariantId { get; init; }
    public ProductVariant ProductVariant { get; init; } = default!;
    public int QuantityToProduce { get; init; }
    public bool IsCustomTailoring { get; init; }
    public bool IsIncomingReturn { get; init; }
    public DateTime? SendUntil { get; init; }
    public string Comment { get; init; }
    public Guid? OrderItemId { get; init; }
}
