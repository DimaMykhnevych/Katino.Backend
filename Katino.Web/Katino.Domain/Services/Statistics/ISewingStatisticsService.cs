using Katino.Domain.Models.Statistics;

namespace Katino.Domain.Services.StatisticsN;

public interface ISewingStatisticsService
{
    Task<SewingStatisticsResult> GetAsync(
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct);
}
