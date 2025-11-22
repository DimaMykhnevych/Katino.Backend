using Katino.Domain.Entities;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Repositories.NovaPoshtaSyncStatusRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.NovaPoshtaSyncStatusRepository;

public class NovaPoshtaSyncStatusRepository : Repository<NovaPoshtaSyncStatus>, INovaPoshtaSyncStatusRepository
{
    public NovaPoshtaSyncStatusRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<NovaPoshtaSyncStatus> GetCurrentSyncStatusAsync(SyncType syncType)
    {
        return await context.NovaPoshtaSyncStatuses
            .Where(s => s.SyncType == syncType)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<NovaPoshtaSyncStatus> GetInProgressSyncByType(SyncType syncType)
    {
        return await context.NovaPoshtaSyncStatuses
            .Where(s =>
                s.SyncType == syncType &&
                s.Status == SyncStatus.InProgress)
            .FirstOrDefaultAsync();
    }

    public async Task<IList<NovaPoshtaSyncStatus>> GetSyncByTypeAndLimitAsync(SyncType? syncType = null, int limit = 20)
    {
        var query = context.NovaPoshtaSyncStatuses.AsQueryable();

        if (syncType.HasValue)
        {
            query = query.Where(s => s.SyncType == syncType.Value);
        }

        return await query
            .OrderByDescending(s => s.StartedAt)
            .Take(limit)
            .ToListAsync();
    }
}
