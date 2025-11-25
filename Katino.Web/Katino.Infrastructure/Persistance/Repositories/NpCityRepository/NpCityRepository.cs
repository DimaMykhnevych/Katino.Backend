using Katino.Domain.Entities;
using Katino.Domain.Repositories.NpCityRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.NpCityRepository;

public class NpCityRepository : Repository<NpCity>, INpCityRepository
{
    public NpCityRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<NpCity> GetNpCityByDeliveryCityAsync(string deliveryCity)
    {
        return await context.NpCities.FirstOrDefaultAsync(c => c.DeliveryCity == deliveryCity);
    }
}
