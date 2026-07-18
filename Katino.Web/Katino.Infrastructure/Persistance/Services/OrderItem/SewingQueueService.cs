using Katino.Domain.Entities;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Domain.Services.OrderItemN.SewingQueueService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderItemN;

public class SewingQueueService : ISewingQueueService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductVariantRedistributionHistoryRepository _historyRepository;
    private readonly ILogger _logger;

    public SewingQueueService(
        IOrderItemRepository orderItemRepository,
        IProductVariantRedistributionHistoryRepository historyRepository,
        ILoggerFactory loggerFactory)
    {
        _orderItemRepository = orderItemRepository;
        _historyRepository = historyRepository;
        _logger = loggerFactory?.CreateLogger(nameof(SewingQueueService));
    }

    public async Task<List<SewingQueueItem>> GetSewingQueueAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting sewing queue");
        var items = await _orderItemRepository.GetOrderItemsForSewingAsync(sewerId, ct);
        var pendingReturns = await _historyRepository.GetPendingIncomingReturnsAsync(sewerId, ct);

        var combined = items.Select(ToSewingQueueItem).Concat(pendingReturns.Select(ToSewingQueueItem));
        return SewingQueueGroupingHelper.Group(combined);
    }

    public async Task<Dictionary<DateTime, List<SewingQueueItem>>> GetSewingQueueGroupedByDateAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting sewing queue grouped by date");
        var itemsByDate = await _orderItemRepository.GetOrderItemsForSewingGroupedByDateAsync(sewerId, ct);
        var pendingReturns = await _historyRepository.GetPendingIncomingReturnsAsync(sewerId, ct);

        var entries = itemsByDate
            .SelectMany(g => g.Value.Select(oi => (g.Key, ToSewingQueueItem(oi))))
            .Concat(pendingReturns.Select(h => (h.SendUntilDate.Date, ToSewingQueueItem(h))));

        return SewingQueueGroupingHelper.GroupByDate(entries);
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

    private static SewingQueueItem ToSewingQueueItem(PendingIncomingReturnSummary pendingReturn)
    {
        return new SewingQueueItem
        {
            ProductVariantId = pendingReturn.ProductVariantId,
            ProductVariant = pendingReturn.ProductVariant,
            QuantityToProduce = pendingReturn.RemainingQuantity,
            IsCustomTailoring = false,
            IsIncomingReturn = true,
            SendUntil = pendingReturn.SendUntilDate,
            Comment = null,
            OrderItemId = pendingReturn.TargetOrderItemId
        };
    }
}
