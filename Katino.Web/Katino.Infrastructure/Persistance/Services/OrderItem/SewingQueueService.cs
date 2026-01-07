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

    public async Task<List<SewingQueueItem>> GetSewingQueueAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Getting sewing queue");
        var items = await _orderItemRepository.GetOrderItemsForSewingAsync(ct);

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

        return grouped
            .OrderByDescending(x => x.QuantityToProduce)
            .Concat(custom.OrderBy(x => x.ProductVariant.Article))
            .ToList();
    }
}
