using Katino.Domain.Services.NovaPost.Sync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class NovaPoshtaSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private static bool _isFirstSyncOnStartup = true;

    public NovaPoshtaSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = loggerFactory?.CreateLogger(nameof(NovaPoshtaSyncBackgroundService));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Nova Poshta Sync Background Service started.");

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider
                    .GetRequiredService<INovaPoshtaSyncService>();

                var isSyncCompleted = await syncService.IsSyncCompletedAsync();

                if (!isSyncCompleted || !_isFirstSyncOnStartup)
                {
                    _logger.LogInformation("Starting initial Nova Poshta data sync...");
                    await syncService.SyncAllDataAsync();
                    _logger.LogInformation("Initial sync completed successfully.");
                }

                _isFirstSyncOnStartup = false;

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Nova Poshta sync background service");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}

