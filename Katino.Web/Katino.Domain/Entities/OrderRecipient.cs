namespace Katino.Domain.Entities;

public class OrderRecipient
{
    public Guid Id { get; set; }
    public string InstUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid NpContactPersonId { get; set; }

    public virtual NpContactPerson NpContactPerson { get; set; }
    public List<Order> Orders { get; set; } = [];
}
