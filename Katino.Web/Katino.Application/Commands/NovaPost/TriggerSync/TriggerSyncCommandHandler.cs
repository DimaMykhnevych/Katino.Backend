using AutoMapper;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Services.NovaPost.Sync;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.NovaPost.TriggerSync;

public class TriggerSyncCommandHandler : IRequestHandler<TriggerSyncCommand, bool>
{
    private readonly INovaPoshtaSyncStatusService _novaPoshtaSyncStatusService;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TriggerSyncCommandHandler(
        INovaPoshtaSyncStatusService novaPoshtaSyncStatus,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IServiceScopeFactory serviceScopeFactory)
    {
        _novaPoshtaSyncStatusService = novaPoshtaSyncStatus;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(TriggerSyncCommandHandler));
        _scopeFactory = serviceScopeFactory;
    }

    public async Task<bool> Handle(TriggerSyncCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling trigger NP sync request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var syncType = _mapper.Map<SyncType>(request.SyncType);
            var syncStatus = await _novaPoshtaSyncStatusService.StartSyncAsync(syncType, request.TriggeredBy);

            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<INovaPoshtaSyncService>();
                var logger = scope.ServiceProvider.GetService<ILogger<TriggerSyncCommandHandler>>();

                try
                {
                    logger?.LogInformation("Starting background sync, SyncId: {SyncId}", syncStatus.Id);

                    await syncService.SyncWarehousesAsync(syncStatus.Id);

                    logger?.LogInformation("Background sync completed, SyncId: {SyncId}", syncStatus.Id);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error in background sync, SyncId: {SyncId}", syncStatus.Id);
                    var statusService = scope.ServiceProvider.GetRequiredService<INovaPoshtaSyncStatusService>();
                    await statusService.FailSyncAsync(syncStatus.Id, ex.Message);
                }
            });

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during triggering NP sync request");
            return false;
        }
    }

}

