using Katino.Domain.Entities;
using Katino.Domain.Repositories.CrmUserSettingsRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.CrmUserSettingsRepository;

public class CrmUserSettingsRepository : Repository<CrmUserSettings>, ICrmUserSettingsRepository
{
    public CrmUserSettingsRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<CrmUserSettings> GetAppUserSettingsWithFullInfo(Guid appUserId)
    {
        return await context.CrmUserSettings
            .Include(x => x.NpCity)
            .AsNoTracking()
            .Include(x => x.NpWarehouse)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.AppUserId == appUserId);
    }
}
