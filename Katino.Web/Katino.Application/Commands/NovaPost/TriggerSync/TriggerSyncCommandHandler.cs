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
            if (await _novaPoshtaSyncStatusService.IsSyncInProgressAsync(syncType))
            {
                return false;
            }

            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();

                var syncService = scope.ServiceProvider.GetRequiredService<INovaPoshtaSyncService>();
                var logger = scope.ServiceProvider.GetService<ILogger<TriggerSyncCommandHandler>>();

                try
                {
                    logger.LogInformation("Starting background sync");
                    await syncService.SyncAllDataAsync(request.TriggeredBy);
                    logger.LogInformation("Background sync completed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in manual sync");
                }
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during triggering NP sync request");
            return false;
        }
    }
}

