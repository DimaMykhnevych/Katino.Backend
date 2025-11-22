using AutoMapper;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Services.NovaPost.Sync;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.NovaPost.TriggerSync;

public class TriggerSyncCommandHandler : IRequestHandler<TriggerSyncCommand, bool>
{
    private readonly INovaPoshtaSyncStatusService _novaPoshtaSyncStatusService;
    private readonly INovaPoshtaSyncService _syncService;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public TriggerSyncCommandHandler(
    INovaPoshtaSyncStatusService novaPoshtaSyncStatus,
    INovaPoshtaSyncService novaPoshtaSyncService,
    IMapper mapper,
    ILoggerFactory loggerFactory)
    {
        _novaPoshtaSyncStatusService = novaPoshtaSyncStatus;
        _syncService = novaPoshtaSyncService;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(TriggerSyncCommandHandler));
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

            // TODO fix and test
            _ = Task.Run(async () =>
            {
                try
                {
                    await _syncService.SyncAllDataAsync(request.TriggeredBy);
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

