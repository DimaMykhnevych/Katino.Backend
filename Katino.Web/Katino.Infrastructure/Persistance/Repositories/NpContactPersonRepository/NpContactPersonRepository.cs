using Katino.Domain.Entities;
using Katino.Domain.Repositories.NpContactPersonRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.NpContactPersonRepository;

public class NpContactPersonRepository : Repository<NpContactPerson>, INpContactPersonRepository
{
    public NpContactPersonRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<NpContactPerson> GetNpContactPersonByRefsAsync(string counterpartyRef, string contactPersonRef)
    {
        return await context.NpContactPersons
            .FirstOrDefaultAsync(cp => cp.CounterpartyRef == counterpartyRef && cp.Ref == contactPersonRef);
    }
}
