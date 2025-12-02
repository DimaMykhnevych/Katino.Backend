using Katino.Domain.Entities;

namespace Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;

public interface IAddOrderRecipientService
{
    Task<Guid> UpsertOrderRecipientAsync(OrderRecipient orderRecipient);
}
