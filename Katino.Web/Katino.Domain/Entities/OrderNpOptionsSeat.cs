namespace Katino.Domain.Entities;

public class OrderNpOptionsSeat
{
    public Guid Id { get; set; }
    public Guid NpOptionsSeatId { get; set; }
    public Guid OrderId { get; set; }

    // Navigation properties
    public NpOptionsSeat NpOptionsSeat { get; set; }
    public Order Order { get; set; }
}
