using Katino.Domain.Models.Statistics;

namespace Katino.Domain.Services.StatisticsN;

public interface IProductSalesRankingService
{
    Task<TopSellingProductsResult> GetAsync(
        OrderStatusFilter statusFilter,
        int page,
        int pageSize,
        DateTimeOffset? from,
        DateTimeOffset? to,
        CancellationToken ct);
}
