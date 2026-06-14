using Katino.Domain.Context;
using Katino.Domain.Models.Statistics;
using Katino.Domain.Services.StatisticsN;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.StatisticsN;

public class SewingStatisticsService : ISewingStatisticsService
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;

    public SewingStatisticsService(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(SewingStatisticsService));
    }

    public async Task<SewingStatisticsResult> GetAsync(
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting sewing statistics");

        var query = _katinoDbContext.SewingHistory.AsNoTracking();

        if (from.HasValue)
        {
            query = query.Where(sh => sh.SewedDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(sh => sh.SewedDate <= to.Value);
        }

        var grouped = query
            .GroupBy(sh => new
            {
                sh.ProductVariant.ProductId,
                sh.ProductVariant.Product.Name,
                sh.SewedBy,
                sh.SewedByUser.UserName
            })
            .Select(g => new SewingStatisticsItem
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                SewerId = g.Key.SewedBy,
                SewerName = g.Key.UserName,
                TotalSewed = g.Sum(sh => sh.SewedQuantity)
            })
            .OrderByDescending(x => x.TotalSewed)
            .ThenBy(x => x.ProductName);

        var totalCount = await grouped.CountAsync(ct);

        var items = await grouped
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new SewingStatisticsResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
