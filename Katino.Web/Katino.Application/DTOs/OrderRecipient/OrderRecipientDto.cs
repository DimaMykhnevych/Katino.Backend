using Katino.Application.DTOs.NpContactPerson;

namespace Katino.Application.DTOs.OrderRecipient;

public class OrderRecipientDto
{
    public Guid Id { get; set; }
    public string InstUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid NpContactPersonId { get; set; }

    public NpContactPersonDto NpContactPerson { get; set; }
}
