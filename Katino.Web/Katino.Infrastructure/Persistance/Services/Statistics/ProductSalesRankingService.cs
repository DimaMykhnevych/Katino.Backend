using Katino.Domain.Context;
using Katino.Domain.Models.Statistics;
using Katino.Domain.Services.StatisticsN;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.StatisticsN;

public class ProductSalesRankingService : IProductSalesRankingService
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;

    public ProductSalesRankingService(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(ProductSalesRankingService));
    }

    public async Task<TopSellingProductsResult> GetAsync(
        OrderStatusFilter statusFilter,
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(statusFilter);
        _logger.LogInformation("Getting product sales ranking");

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var statuses = statusFilter.Statuses.ToList();

        var query = _katinoDbContext.OrderItems.AsNoTracking();

        query = statusFilter.IsExclusion
            ? query.Where(oi => !statuses.Contains(oi.Order.OrderStatus))
            : query.Where(oi => statuses.Contains(oi.Order.OrderStatus));

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
