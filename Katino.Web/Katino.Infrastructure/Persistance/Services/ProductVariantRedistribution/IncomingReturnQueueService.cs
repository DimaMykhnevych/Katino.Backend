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

        var history = await _historyRepository.GetPendingIncomingReturnsAsync(ct);
        if (history.Count == 0)
        {
            return new Dictionary<DateTime, List<SewingQueueItem>>();
        }

        var targetOrderItemIds = history.Select(h => h.TargetOrderItemId.Value);
        var orderItemsById = await _orderItemRepository.GetByIdsAsync(targetOrderItemIds, ct);

        var entries = history
            .Where(h => orderItemsById.ContainsKey(h.TargetOrderItemId.Value))
            .Select(h =>
            {
                var orderItem = orderItemsById[h.TargetOrderItemId.Value];
                var item = new SewingQueueItem
                {
                    ProductVariantId = h.ProductVariantId,
                    ProductVariant = h.ProductVariant,
                    QuantityToProduce = h.Quantity,
                    IsCustomTailoring = orderItem.IsCustomTailoring,
                    Comment = orderItem.Comment,
                    OrderItemId = orderItem.Id
                };

                return (Date: h.TargetOrder.SendUntilDate.Date, Item: item);
            });

        return SewingQueueGroupingHelper.GroupByDate(entries);
    }
}
