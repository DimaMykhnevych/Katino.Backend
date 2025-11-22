using Katino.Domain.Entities;
using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Repositories.NovaPoshtaSyncStatusRepository;

public interface INovaPoshtaSyncStatusRepository : IRepository<NovaPoshtaSyncStatus>
{
    Task<NovaPoshtaSyncStatus> GetCurrentSyncStatusAsync(SyncType syncType);
    Task<NovaPoshtaSyncStatus> GetInProgressSyncByType(SyncType syncType);
    Task<IList<NovaPoshtaSyncStatus>> GetSyncByTypeAndLimitAsync(SyncType? syncType = null, int limit = 20);
}
