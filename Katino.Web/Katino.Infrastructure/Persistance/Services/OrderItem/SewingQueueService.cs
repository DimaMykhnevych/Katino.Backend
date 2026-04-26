using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Services.OrderItemN.SewingQueueService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderItemN;

public class SewingQueueService : ISewingQueueService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ILogger _logger;

    public SewingQueueService(
        IOrderItemRepository orderItemRepository,
        ILoggerFactory loggerFactory)
    {
        _orderItemRepository = orderItemRepository;
        _logger = loggerFactory?.CreateLogger(nameof(SewingQueueService));
    }

    public async Task<List<SewingQueueItem>> GetSewingQueueAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting sewing queue");
        var items = await _orderItemRepository.GetOrderItemsForSewingAsync(sewerId, ct);

        var custom = items
            .Where(x => x.IsCustomTailoring)
            .Select(x => new SewingQueueItem
            {
                ProductVariantId = x.ProductVariantId,
                ProductVariant = x.ProductVariant,
                QuantityToProduce = x.QuantityToProduce,
                IsCustomTailoring = true,
                Comment = x.Comment,
                OrderItemId = x.Id
            })
            .ToList();

        _logger.LogDebug($"Custom order items count: {custom.Count}");

        var grouped = items
            .Where(x => !x.IsCustomTailoring)
            .GroupBy(x => x.ProductVariantId)
            .Select(g =>
            {
                var first = g.First();
                return new SewingQueueItem
                {
                    ProductVariantId = g.Key,
                    ProductVariant = first.ProductVariant,
                    QuantityToProduce = g.Sum(x => x.QuantityToProduce),
                    IsCustomTailoring = false,
                    Comment = null,
                    OrderItemId = null
                };
            })
            .ToList();

        var combined = grouped.Concat(custom).ToList();

        return combined
            .OrderBy(i => i.ProductVariant.Product.Name)
            .ThenBy(i => i.ProductVariant.Color.Name)
            .ThenBy(i => SortHelper.GetSizeSortGroup(i.ProductVariant.Size.Name))
            .ThenBy(i => SortHelper.GetSizeSortValue(i.ProductVariant.Size.Name))
            .ThenBy(i => SortHelper.NormalizeSizeName(i.ProductVariant.Size.Name))
            .ToList();
    }

    public async Task<Dictionary<DateTime, List<SewingQueueItem>>> GetSewingQueueGroupedByDateAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting sewing queue grouped by date");
        var items = await _orderItemRepository.GetOrderItemsForSewingGroupedByDateAsync(sewerId, ct);

        return items.ToDictionary(
            x => x.Key,
            x => x.Value
                .Select(oi => new SewingQueueItem
                {
                    ProductVariantId = oi.ProductVariantId,
                    ProductVariant = oi.ProductVariant,
                    QuantityToProduce = oi.QuantityToProduce,
                    IsCustomTailoring = oi.IsCustomTailoring,
                    Comment = oi.Comment,
                    OrderItemId = oi.Id
                })
                .OrderBy(i => i.ProductVariant.Product.Name)
                .ThenBy(i => i.ProductVariant.Color.Name)
                .ThenBy(i => SortHelper.GetSizeSortGroup(i.ProductVariant.Size.Name))
                .ThenBy(i => SortHelper.GetSizeSortValue(i.ProductVariant.Size.Name))
                .ThenBy(i => SortHelper.NormalizeSizeName(i.ProductVariant.Size.Name))
                .ToList());
    }
}
