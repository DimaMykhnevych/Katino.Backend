using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Domain.Services.ProductVariantRedistributionN.IncomingReturnQueueService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.ProductVariantRedistributionN;

public class IncomingReturnQueueService : IIncomingReturnQueueService
{
    private readonly IProductVariantRedistributionHistoryRepository _historyRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ILogger _logger;

    public IncomingReturnQueueService(
        IProductVariantRedistributionHistoryRepository historyRepository,
        IOrderItemRepository orderItemRepository,
        ILoggerFactory loggerFactory)
    {
        _historyRepository = historyRepository;
        _orderItemRepository = orderItemRepository;
        _logger = loggerFactory?.CreateLogger(nameof(IncomingReturnQueueService));
    }

    public async Task<Dictionary<DateTime, List<SewingQueueItem>>> GetIncomingReturnQueueGroupedByDateAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Getting incoming return queue grouped by date");

        var pendingReturns = await _historyRepository.GetPendingIncomingReturnsAsync(sewerId: null, ct: ct);
        if (pendingReturns.Count == 0)
        {
            return new Dictionary<DateTime, List<SewingQueueItem>>();
        }

        var targetOrderItemIds = pendingReturns.Select(h => h.TargetOrderItemId);
        var orderItemsById = await _orderItemRepository.GetByIdsAsync(targetOrderItemIds, ct);

        var entries = pendingReturns
            .Where(h => orderItemsById.ContainsKey(h.TargetOrderItemId))
            .Select(h =>
            {
                var orderItem = orderItemsById[h.TargetOrderItemId];
                var item = new SewingQueueItem
                {
                    ProductVariantId = h.ProductVariantId,
                    ProductVariant = h.ProductVariant,
                    QuantityToProduce = h.RemainingQuantity,
                    IsCustomTailoring = orderItem.IsCustomTailoring,
                    IsIncomingReturn = true,
                    SendUntil = h.SendUntilDate,
                    Comment = orderItem.Comment,
                    OrderItemId = orderItem.Id
                };

                return (Date: h.SendUntilDate.Date, Item: item);
            });

        return SewingQueueGroupingHelper.GroupByDate(entries);
    }
}
