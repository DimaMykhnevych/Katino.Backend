using Katino.Functions.Services.NpIntDocStatusSyncService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Katino.Functions.Functions;

public class NpStatusSync
{
    private readonly INpIntDocStatusSyncService _npIntDocStatusSyncService;
    private readonly ILogger _logger;

    public NpStatusSync(
        INpIntDocStatusSyncService npIntDocStatusSyncService,
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<NpStatusSync>();
        _npIntDocStatusSyncService = npIntDocStatusSyncService;
    }

    [Function("RunNpStatusSync")]
    public async Task Run([TimerTrigger("%NpStatusSyncTimerSchedule%")] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function RunNpStatusSync executed at: {DateTime.Now}");

        await _npIntDocStatusSyncService.RunSync();

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next call of RunNpStatusSync at: {myTimer.ScheduleStatus.Next}");
        }
    }
}
