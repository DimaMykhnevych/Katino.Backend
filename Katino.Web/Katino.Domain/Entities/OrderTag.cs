using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class OrderTag
{
    public Guid Id { get; set; }
    public OrderTagType Type { get; set; }
    public bool CanBeDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderOrderTag> OrderOrderTags { get; set; } = [];
}
