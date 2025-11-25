using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.NpCityRepository;

public interface INpCityRepository : IRepository<NpCity>
{
    Task<NpCity> GetNpCityByDeliveryCityAsync(string deliveryCity);
}
