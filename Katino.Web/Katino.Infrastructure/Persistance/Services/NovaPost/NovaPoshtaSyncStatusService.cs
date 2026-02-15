using Katino.Domain.Entities;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Repositories.NovaPoshtaSyncStatusRepository;
using Katino.Domain.Services.NovaPost.Sync;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class NovaPoshtaSyncStatusService : INovaPoshtaSyncStatusService
{
    private const int SyncStuckMinutes = 45;
    private readonly INovaPoshtaSyncStatusRepository _novaPoshtaSyncStatusRepository;
    private readonly ILogger _logger;

    public NovaPoshtaSyncStatusService(
        INovaPoshtaSyncStatusRepository novaPoshtaSyncStatusRepository,
        ILoggerFactory loggerFactory)
    {
        _novaPoshtaSyncStatusRepository = novaPoshtaSyncStatusRepository;
        _logger = loggerFactory?.CreateLogger(nameof(NovaPoshtaSyncStatusService));
    }

    public async Task<NovaPoshtaSyncStatus> GetCurrentSyncStatusAsync(SyncType syncType)
    {
        return await _novaPoshtaSyncStatusRepository.GetCurrentSyncStatusAsync(syncType);
    }

    public async Task<bool> IsSyncInProgressAsync(SyncType syncType)
    {
        var activeSync = await _novaPoshtaSyncStatusRepository.GetInProgressSyncByType(syncType);

        if (activeSync == null)
        {
            return false;
        }

        var timeoutThreshold = DateTimeOffset.UtcNow.AddMinutes(-SyncStuckMinutes);
        if (activeSync.StartedAt < timeoutThreshold)
        {
            _logger.LogWarning(
                $"Sync {activeSync.Id} appears to be stuck. Started at {activeSync.StartedAt}");

            await FailSyncAsync(activeSync.Id, "Sync timeout - exceeded 2 hours");
            return false;
        }

        return true;
    }

    public async Task<NovaPoshtaSyncStatus> StartSyncAsync(SyncType syncType, Guid triggeredBy)
    {
        if (await IsSyncInProgressAsync(syncType))
        {
            throw new InvalidOperationException(
                $"Sync of type {syncType} is already in progress");
        }

        var syncStatus = new NovaPoshtaSyncStatus
        {
            SyncType = syncType,
            Status = SyncStatus.InProgress,
            StartedAt = DateTimeOffset.UtcNow,
            TriggeredBy = triggeredBy,
            ApiRequestedRecords = 0,
            DbInsertedRecords = 0
        };

        await _novaPoshtaSyncStatusRepository.Insert(syncStatus);
        await _novaPoshtaSyncStatusRepository.Save();

        _logger.LogInformation(
            $"Sync {syncStatus.Id} started. Type: {syncType}, Triggered by: {triggeredBy}");

        return syncStatus;
    }

    public async Task UpdateSyncProgressAsync(Guid syncId, int apiRequestedRecords, int dbInsertedRecords)
    {
        var syncStatus = await _novaPoshtaSyncStatusRepository.Get(syncId);

        if (syncStatus == null)
        {
            _logger.LogWarning($"Sync {syncId} not found for progress update");
            return;
        }

        syncStatus.ApiRequestedRecords = apiRequestedRecords;
        syncStatus.DbInsertedRecords = dbInsertedRecords;

        await _novaPoshtaSyncStatusRepository.Update(syncStatus);
        await _novaPoshtaSyncStatusRepository.Save();
    }

    public async Task CompleteSyncAsync(Guid syncId, int dbInsertedRecords)
    {
        var syncStatus = await _novaPoshtaSyncStatusRepository.Get(syncId);

        if (syncStatus == null)
        {
            _logger.LogWarning($"Sync {syncId} not found for completion");
            return;
        }

        syncStatus.Status = SyncStatus.Completed;
        syncStatus.CompletedAt = DateTimeOffset.UtcNow;
        syncStatus.DbInsertedRecords = dbInsertedRecords;

        await _novaPoshtaSyncStatusRepository.Update(syncStatus);
        await _novaPoshtaSyncStatusRepository.Save();

        var duration = syncStatus.CompletedAt.Value - syncStatus.StartedAt.Value;
        _logger.LogInformation(
            $"Sync {syncId} completed. Type: {syncStatus.SyncType}, " +
            $"Records: {dbInsertedRecords}, Duration: {duration.TotalMinutes:F2} minutes");
    }

    public async Task FailSyncAsync(Guid syncId, string errorMessage)
    {
        var syncStatus = await _novaPoshtaSyncStatusRepository.Get(syncId);

        if (syncStatus == null)
        {
            _logger.LogWarning($"Sync {syncId} not found for failure");
            return;
        }

        syncStatus.Status = SyncStatus.Failed;
        syncStatus.CompletedAt = DateTimeOffset.UtcNow;
        syncStatus.ErrorMessage = errorMessage;

        await _novaPoshtaSyncStatusRepository.Update(syncStatus);
        await _novaPoshtaSyncStatusRepository.Save();

        _logger.LogError($"Sync {syncId} failed. Error: {errorMessage}");
    }

    public async Task<IList<NovaPoshtaSyncStatus>> GetSyncHistoryAsync(
        SyncType? syncType = null,
        int limit = 20)
    {
        return await _novaPoshtaSyncStatusRepository.GetSyncByTypeAndLimitAsync(syncType, limit);
    }
}
