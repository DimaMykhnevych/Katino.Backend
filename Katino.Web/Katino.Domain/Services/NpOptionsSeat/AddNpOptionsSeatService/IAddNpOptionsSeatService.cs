using Katino.Domain.Entities;

namespace Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;

public interface IAddNpOptionsSeatService
{
    Task<Guid> GetOrCreateNpOptionsSeat(NpOptionsSeat npOptionsSeat);
}
