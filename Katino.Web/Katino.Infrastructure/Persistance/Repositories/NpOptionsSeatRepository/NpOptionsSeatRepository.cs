using Katino.Domain.Entities;
using Katino.Domain.Repositories.NpOptionsSeatRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.NpOptionsSeatRepository;

public class NpOptionsSeatRepository : Repository<NpOptionsSeat>, INpOptionsSeatRepository
{
    public NpOptionsSeatRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<NpOptionsSeat> GetBySeatParamsAsync(NpOptionsSeat npOptionsSeat)
    {
        return await context.NpOptionsSeats
            .FirstOrDefaultAsync(f => f.VolumetricWidth == npOptionsSeat.VolumetricWidth &&
                                      f.VolumetricLength == npOptionsSeat.VolumetricLength &&
                                      f.VolumetricHeight == npOptionsSeat.VolumetricHeight &&
                                      f.Weight == npOptionsSeat.Weight);
    }
}
