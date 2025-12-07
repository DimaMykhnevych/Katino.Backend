using Katino.Application.DTOs.NpContactPerson;

namespace Katino.Application.DTOs.OrderRecipient;

public class AddOrderRecipientDto
{
    public string InstUrl { get; set; }

    public AddNpContactPersonDto NpContactPerson { get; set; }
}
