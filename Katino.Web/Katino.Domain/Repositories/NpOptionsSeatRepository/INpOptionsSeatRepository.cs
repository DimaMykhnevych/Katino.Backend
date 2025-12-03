using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.NpOptionsSeatRepository;

public interface INpOptionsSeatRepository : IRepository<NpOptionsSeat>
{
    Task<NpOptionsSeat> GetBySeatParamsAsync(NpOptionsSeat npOptionsSeat);
}
