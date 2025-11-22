using Katino.Domain.Entities;
using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Services.NovaPost.Sync;

public interface INovaPoshtaSyncStatusService
{
    Task<NovaPoshtaSyncStatus> GetCurrentSyncStatusAsync(SyncType syncType);
    Task<bool> IsSyncInProgressAsync(SyncType syncType);
    Task<NovaPoshtaSyncStatus> StartSyncAsync(SyncType syncType, Guid triggeredBy);
    Task UpdateSyncProgressAsync(Guid syncId, int apiRequestedRecords, int dbInsertedRecords);
    Task CompleteSyncAsync(Guid syncId, int dbInsertedRecords);
    Task FailSyncAsync(Guid syncId, string errorMessage);
    Task<IList<NovaPoshtaSyncStatus>> GetSyncHistoryAsync(SyncType? syncType = null, int limit = 20);
}
