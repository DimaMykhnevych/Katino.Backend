using Katino.Domain.Context;
using Katino.Domain.Enums;
using Katino.Domain.Models.Statistics;
using Katino.Domain.Services.StatisticsN;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.StatisticsN;

public class TopSellingProductsService : ITopSellingProductsService
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;

    public TopSellingProductsService(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(TopSellingProductsService));
    }

    public async Task<TopSellingProductsResult> GetAsync(
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting top selling products");

        var query = _katinoDbContext.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.OrderStatus == OrderStatus.Received);

        if (from.HasValue)
        {
            query = query.Where(oi => oi.Order.CreationDateTime >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(oi => oi.Order.CreationDateTime <= to.Value);
        }

        var grouped = query
            .GroupBy(oi => new { oi.ProductVariant.ProductId, oi.ProductVariant.Product.Name })
            .Select(g => new TopSellingProductItem
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                TotalSold = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(x => x.TotalSold);

        var totalCount = await grouped.CountAsync(ct);

        var products = await grouped
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new TopSellingProductsResult
        {
            Products = products,
            TotalCount = totalCount
        };
    }
}
