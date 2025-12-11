using Katino.Application.DTOs.NpContactPerson;

namespace Katino.Application.DTOs.OrderRecipient;

public class UpdateOrderRecipientDto
{
    public string InstUrl { get; set; }

    public UpdateNpContactPersonDto NpContactPerson { get; set; }
}
