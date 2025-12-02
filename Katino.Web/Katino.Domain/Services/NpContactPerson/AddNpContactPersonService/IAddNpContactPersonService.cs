using Katino.Domain.Entities;

namespace Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;

public interface IAddNpContactPersonService
{
    Task<Guid> UpsertNpContactPersonAsync(NpContactPerson npContactPerson);
}
