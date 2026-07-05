using Katino.Domain.Entities;
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

        return SewingQueueGroupingHelper.Group(items.Select(ToSewingQueueItem));
    }

    public async Task<Dictionary<DateTime, List<SewingQueueItem>>> GetSewingQueueGroupedByDateAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting sewing queue grouped by date");
        var items = await _orderItemRepository.GetOrderItemsForSewingGroupedByDateAsync(sewerId, ct);

        return items.ToDictionary(
            x => x.Key,
            x => SewingQueueGroupingHelper.Group(x.Value.Select(ToSewingQueueItem)));
    }

    private static SewingQueueItem ToSewingQueueItem(OrderItem orderItem)
    {
        return new SewingQueueItem
        {
            ProductVariantId = orderItem.ProductVariantId,
            ProductVariant = orderItem.ProductVariant,
            QuantityToProduce = orderItem.QuantityToProduce,
            IsCustomTailoring = orderItem.IsCustomTailoring,
            Comment = orderItem.Comment,
            OrderItemId = orderItem.Id
        };
    }
}
