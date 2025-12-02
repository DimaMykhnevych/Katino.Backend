using Katino.Domain.Entities;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.OrderRecipientRepository;

public class OrderRecipientRepository : Repository<OrderRecipient>, IOrderRecipientRepository
{
    public OrderRecipientRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<OrderRecipient> GetOrderRecipientByPhoneNumberAsync(string phoneNumber)
    {
        return await context.OrderRecipients
            .Include(or => or.NpContactPerson)
            .AsNoTracking()
            .FirstOrDefaultAsync(or => or.NpContactPerson.Phones == phoneNumber);
    }
}
