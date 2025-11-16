using Katino.Domain.Entities;
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
    private readonly ILogger _logger;

    public NovaPoshtaSyncService(
        IWarehouseService warehouseService,
        INpWarehouseRepository npWarehouseRepository,
        ILoggerFactory loggerFactory)
    {
        _warehouseService = warehouseService;
        _npWarehouseRepository = npWarehouseRepository;
        _logger = loggerFactory?.CreateLogger(nameof(NovaPoshtaSyncService));
    }

    public async Task<bool> IsSyncCompletedAsync()
    {
        var warehouseCount = await _npWarehouseRepository.GetWarehousesCountAsync();
        return warehouseCount > 0;
    }

    public async Task SyncAllDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting Nova Poshta data synchronization...");

            await SyncWarehousesAsync();

            _logger.LogInformation("Nova Poshta data synchronization completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Nova Poshta data synchronization");
            throw;
        }
    }

    private async Task SyncWarehousesAsync()
    {
        _logger.LogInformation("Syncing warehouses...");

        var allWarehouses = new List<WarehousesResponse>();
        var page = 1;
        var hasMorePages = true;

        while (hasMorePages)
        {
            try
            {
                var response = await _warehouseService.GetWarehousesWithPaginationAsync(page.ToString(), WarehousesLimit);

                if (response.Any())
                {
                    allWarehouses.AddRange(response);
                    _logger.LogInformation($"Downloaded {response.Count()} warehouses (page {page})");
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

        foreach (var warehouse in allWarehouses)
        {
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

        }

        await _npWarehouseRepository.Save();
        _logger.LogInformation("Warehouses sync completed.");

        var closedWarehousesCount = await _npWarehouseRepository.GetInactiveWarehousesCountAsync();

        _logger.LogWarning($"Found {closedWarehousesCount} closed warehouses");
    }
}
