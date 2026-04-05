namespace Katino.Domain.Entities;

public class OrderOrderTag
{
    public Guid OrderId { get; set; }
    public Guid OrderTagId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; }
    public OrderTag OrderTag { get; set; }
}
