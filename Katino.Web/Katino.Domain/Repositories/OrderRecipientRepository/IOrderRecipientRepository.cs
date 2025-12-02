using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.OrderRecipientRepository;

public interface IOrderRecipientRepository : IRepository<OrderRecipient>
{
    Task<OrderRecipient> GetOrderRecipientByPhoneNumberAsync(string phoneNumber);
}
