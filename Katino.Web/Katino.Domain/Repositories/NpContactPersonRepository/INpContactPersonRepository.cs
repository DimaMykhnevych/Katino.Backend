using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.NpContactPersonRepository;

public interface INpContactPersonRepository : IRepository<NpContactPerson>
{
    Task<NpContactPerson> GetNpContactPersonByRefsAsync(string counterpartyRef, string contactPersonRef);
}
