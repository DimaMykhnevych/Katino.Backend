namespace Katino.Application.DTOs.OrderTag;

public class OrderTagDto
{
    public Guid Id { get; set; }
    public OrderTagTypeDto Type { get; set; }
    public bool CanBeDeleted { get; set; }
    public string Value { get; set; }
}
