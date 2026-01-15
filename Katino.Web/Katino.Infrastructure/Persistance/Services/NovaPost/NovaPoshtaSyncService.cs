using Katino.Domain.Entities;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Repositories.NpWarehouseRepository;
using Katino.Domain.Services.NovaPost.Sync;
using Katino.Domain.Services.NovaPost.Warehouse;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class NovaPoshtaSyncService : INovaPoshtaSyncService
{
    private const string WarehousesLimit = "1000";
    private const int RateLimitDelayMs = 500;

    private readonly IWarehouseService _warehouseService;
    private readonly INpWarehouseRepository _npWarehouseRepository;
    private readonly INovaPoshtaSyncStatusService _syncStatusService;
    private readonly ILogger _logger;

    public NovaPoshtaSyncService(
        IWarehouseService warehouseService,
        INpWarehouseRepository npWarehouseRepository,
        INovaPoshtaSyncStatusService novaPoshtaSyncStatusService,
        ILoggerFactory loggerFactory)
    {
        _warehouseService = warehouseService;
        _npWarehouseRepository = npWarehouseRepository;
        _syncStatusService = novaPoshtaSyncStatusService;
        _logger = loggerFactory?.CreateLogger(nameof(NovaPoshtaSyncService));
    }

    public async Task<bool> IsSyncCompletedAsync()
    {
        var warehouseCount = await _npWarehouseRepository.GetWarehousesCountAsync();
        return warehouseCount > 0;
    }

    public async Task SyncAllDataAsync(Guid triggeredBy)
    {
        if (await _syncStatusService.IsSyncInProgressAsync(SyncType.Warehouses))
        {
            _logger.LogWarning("Sync already in progress, skipping");
            return;
        }

        NovaPoshtaSyncStatus syncStatus = null;

        try
        {
            syncStatus = await _syncStatusService.StartSyncAsync(SyncType.Warehouses, triggeredBy);

            _logger.LogInformation("Starting Nova Poshta data synchronization...");

            var totalRecordsInserted = await SyncWarehousesAsyncInternal(syncStatus.Id);

            await _syncStatusService.CompleteSyncAsync(syncStatus.Id, totalRecordsInserted);

            _logger.LogInformation("Nova Poshta data synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Nova Poshta data synchronization");

            if (syncStatus != null)
            {
                await _syncStatusService.FailSyncAsync(syncStatus.Id, ex.Message);
            }

            throw;
        }
    }

    public async Task SyncWarehousesAsync(Guid syncStatusId)
    {
        try
        {
            _logger.LogInformation("Starting Nova Poshta warehouses sync. SyncId: {SyncId}", syncStatusId);

            var totalRecordsInserted = await SyncWarehousesAsyncInternal(syncStatusId);

            await _syncStatusService.CompleteSyncAsync(syncStatusId, totalRecordsInserted);

            _logger.LogInformation("Sync completed successfully. SyncId: {SyncId}", syncStatusId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sync. SyncId: {SyncId}", syncStatusId);
            await _syncStatusService.FailSyncAsync(syncStatusId, ex.Message);
            throw;
        }
    }


    private async Task<int> SyncWarehousesAsyncInternal(Guid syncId)
    {
        _logger.LogInformation("Syncing warehouses...");

        await _npWarehouseRepository.SetWarehouseActiveAsync(false);

        var allWarehouses = new List<WarehousesResponse>();
        var page = 1;
        var hasMorePages = true;
        var totalRecordsRequested = 0;

        while (hasMorePages)
        {
            try
            {
                var response = await _warehouseService.GetWarehousesWithPaginationAsync(page.ToString(), WarehousesLimit);

                if (response.Any())
                {
                    allWarehouses.AddRange(response);

                    var responseCount = response.Count();
                    _logger.LogDebug($"Downloaded {responseCount} warehouses (page {page})");

                    totalRecordsRequested += responseCount;

                    await _syncStatusService.UpdateSyncProgressAsync(
                        syncId,
                        totalRecordsRequested,
                        0);

                    page++;

                    await Task.Delay(RateLimitDelayMs);
                }
                else
                {
                    hasMorePages = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading warehouses page {page}");
                throw;
            }
        }

        _logger.LogInformation($"Saving {allWarehouses.Count} warehouses to database...");

        for (var i = 0; i < allWarehouses.Count; i++)
        {
            var warehouse = allWarehouses[i];

            var existingWarehouse = await _npWarehouseRepository.GetWarehouseByRefAsync(warehouse.Ref);


            if (existingWarehouse != null)
            {
                existingWarehouse.CityRef = warehouse.CityRef;
                existingWarehouse.WarehouseIndex = warehouse.WarehouseIndex;
                existingWarehouse.Description = warehouse.Description;
                existingWarehouse.Number = warehouse.Number;
                existingWarehouse.ShortAddress = warehouse.ShortAddress;
                existingWarehouse.IsActive = true;
                existingWarehouse.UpdatedAt = DateTime.UtcNow;

                await _npWarehouseRepository.Update(existingWarehouse);
            }
            else
            {
                await _npWarehouseRepository.Insert(new NpWarehouse
                {
                    Ref = warehouse.Ref,
                    CityRef = warehouse.CityRef,
                    WarehouseIndex = warehouse.WarehouseIndex,
                    Description = warehouse.Description,
                    Number = warehouse.Number,
                    ShortAddress = warehouse.ShortAddress,
                    IsActive = true,
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            if (i != 0 && i % 10_000 == 0)
            {
                _logger.LogTrace($"Saved/updated {i} warehouses/postomats");
                await _syncStatusService.UpdateSyncProgressAsync(
                    syncId,
                    allWarehouses.Count,
                    i);
            }
        }

        await _npWarehouseRepository.Save();
        _logger.LogInformation("Warehouses sync completed.");

        var closedWarehousesCount = await _npWarehouseRepository.GetInactiveWarehousesCountAsync();

        _logger.LogWarning($"Found {closedWarehousesCount} closed warehouses");

        return allWarehouses.Count;
    }
}
