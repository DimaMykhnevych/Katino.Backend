using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Domain.Services.OrderN.UrgentOrderRedistributionService;
using Katino.Domain.Services.ProductVariantRedistributionN.ProductVariantRedistributionRecorder;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class UrgentOrderRedistributionService : IUrgentOrderRedistributionService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductVariantRedistributionRecorder _redistributionRecorder;
    private readonly IProductVariantRedistributionHistoryRepository _redistributionHistoryRepository;
    private readonly IOrderTagRepository _orderTagRepository;
    private readonly ILogger _logger;

    public UrgentOrderRedistributionService(
        IOrderRepository orderRepository,
        IProductVariantRedistributionRecorder redistributionRecorder,
        IProductVariantRedistributionHistoryRepository redistributionHistoryRepository,
        IOrderTagRepository orderTagRepository,
        ILoggerFactory loggerFactory)
    {
        _orderRepository = orderRepository;
        _redistributionRecorder = redistributionRecorder;
        _redistributionHistoryRepository = redistributionHistoryRepository;
        _orderTagRepository = orderTagRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UrgentOrderRedistributionService));
    }

    public async Task RedistributeForUrgentOrderAsync(Order urgentOrder)
    {
        var forSewingItems = urgentOrder.OrderItems
            .Where(i => !i.IsCustomTailoring && i.OrderItemStatus == OrderItemStatus.ForSewing)
            .ToList();

        if (!forSewingItems.Any())
        {
            return;
        }

        _logger.LogInformation($"Attempting redistribution for urgent order {urgentOrder.Id}, {forSewingItems.Count} items need redistribution");

        var affectedOrders = new HashSet<Order>();
        var donorOrdersWithConsumedPhantom = new HashSet<Order>();
        var touchedOrderItems = new HashSet<OrderItem>();
        var redistributionEvents = new List<ProductVariantRedistributionEvent>();

        foreach (var urgentItem in forSewingItems)
        {
            var remainingToFill = urgentItem.QuantityToProduce;
            if (remainingToFill <= 0) continue;

            var lessUrgentOrders = await _orderRepository
                .GetLessUrgentOrdersWithProductVariantAsync(urgentItem.ProductVariantId, urgentOrder.SendUntilDate);

            var candidates = new List<(Order DonorOrder, OrderItem DonorItem, int PhantomAvailable, int RealAvailable)>();
            foreach (var donorOrder in lessUrgentOrders)
            {
                // A donor order can have several items of the same product variant (e.g. one covered by
                // real stock, another still-in-transit) - all must be considered, otherwise picking just
                // one can hide a sibling item's phantom coverage from this donation pass entirely.
                var donorItems = donorOrder.OrderItems
                    .Where(i => i.ProductVariantId == urgentItem.ProductVariantId && !i.IsCustomTailoring);

                foreach (var donorItem in donorItems)
                {
                    var readyQuantity = donorItem.Quantity - donorItem.QuantityToProduce;
                    if (readyQuantity <= 0) continue;

                    var phantomRemaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(donorItem.Id);
                    var phantomAvailable = Math.Min(phantomRemaining, readyQuantity);
                    var realAvailable = readyQuantity - phantomAvailable;

                    candidates.Add((donorOrder, donorItem, phantomAvailable, realAvailable));
                }
            }

            // Prefer taking from donors whose "ready" coverage is itself still in transit (phantom) - that
            // quantity isn't genuinely available anyway, so handing it to a more urgent order loses nothing
            // real. Only once phantom sources are exhausted do we dip into truly finished stock.
            foreach (var takeFromPhantom in new[] { true, false })
            {
                foreach (var candidate in candidates)
                {
                    if (remainingToFill <= 0) break;

                    var available = takeFromPhantom ? candidate.PhantomAvailable : candidate.RealAvailable;
                    if (available <= 0) continue;

                    var quantityToTake = Math.Min(available, remainingToFill);
                    var donorOrder = candidate.DonorOrder;
                    var donorItem = candidate.DonorItem;

                    donorItem.QuantityToProduce += quantityToTake;
                    donorItem.OrderItemStatus = donorItem.QuantityToProduce > 0
                        ? OrderItemStatus.ForSewing
                        : OrderItemStatus.Ready;

                    var newDonorOrderStatus = donorOrder.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
                        ? OrderStatus.InProgress
                        : OrderStatus.ReadyToShip;

                    var previousDonorStatus = donorOrder.OrderStatus;
                    OrderStatusHelper.SetOrderStatus(donorOrder, newDonorOrderStatus, false);
                    if (donorOrder.OrderStatus != previousDonorStatus)
                    {
                        donorOrder.UpdateReasonDetails = $"Items redistributed to urgent order (id: '{urgentOrder.Id}')";
                    }

                    urgentItem.QuantityToProduce -= quantityToTake;
                    urgentItem.OrderItemStatus = urgentItem.QuantityToProduce > 0
                        ? OrderItemStatus.ForSewing
                        : OrderItemStatus.Ready;

                    remainingToFill -= quantityToTake;
                    affectedOrders.Add(donorOrder);
                    touchedOrderItems.Add(donorItem);
                    touchedOrderItems.Add(urgentItem);

                    if (takeFromPhantom)
                    {
                        await _redistributionHistoryRepository.ConsumePendingReturnAsync(donorItem.Id, quantityToTake);
                        donorOrdersWithConsumedPhantom.Add(donorOrder);
                    }

                    redistributionEvents.Add(new ProductVariantRedistributionEvent
                    {
                        ProductVariantId = urgentItem.ProductVariantId,
                        Reason = ProductVariantQuantityChangeReason.UrgentReallocation,
                        SourceOrderId = donorOrder.Id,
                        SourceOrderItemId = donorItem.Id,
                        SourceOrderTtnSnapshot = donorOrder.InternetDocumentIntDocNumber,
                        IsPendingPhysicalArrival = takeFromPhantom,
                        Lines = [
                            new ProductVariantRedistributionLine
                            {
                                TargetOrderId = urgentOrder.Id,
                                TargetOrderItemId = urgentItem.Id,
                                Quantity = quantityToTake
                            }
                        ]
                    });

                    _logger.LogDebug($"Redistributed {quantityToTake} units of product variant {urgentItem.ProductVariantId} from order {donorOrder.Id} to urgent order {urgentOrder.Id} (phantom: {takeFromPhantom})");
                }
            }
        }

        if (!affectedOrders.Any())
        {
            return;
        }

        var utcNow = DateTimeOffset.UtcNow;
        foreach (var affectedOrder in affectedOrders)
        {
            affectedOrder.UpdatedAt = utcNow;
        }

        var newUrgentOrderStatus = urgentOrder.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
            ? OrderStatus.InProgress
            : OrderStatus.ReadyToShip;

        var previousUrgentStatus = urgentOrder.OrderStatus;
        OrderStatusHelper.SetOrderStatus(urgentOrder, newUrgentOrderStatus, false);
        if (urgentOrder.OrderStatus != previousUrgentStatus)
        {
            urgentOrder.UpdatedAt = utcNow;
            urgentOrder.UpdateReasonDetails = "Items redistributed from less urgent orders";
        }

        await _orderRepository.Save();

        foreach (var redistributionEvent in redistributionEvents)
        {
            await _redistributionRecorder.RecordAsync(redistributionEvent);
        }

        // The donor may no longer have any open pending return once its phantom coverage was handed off -
        // drop the PendingIncomingReturn tag so its own order history stays accurate.
        foreach (var donorOrder in donorOrdersWithConsumedPhantom)
        {
            var hasOtherOpenPendingReturns = await _redistributionHistoryRepository.HasOpenPendingReturnsForOrderAsync(donorOrder.Id);
            if (!hasOtherOpenPendingReturns)
            {
                var tag = await _orderTagRepository.GetOrCreateByTypeAsync(OrderTagType.PendingIncomingReturn, canBeDeleted: true);
                if (await _orderTagRepository.IsTagAttachedToOrderAsync(donorOrder.Id, tag.Id))
                {
                    await _orderTagRepository.DetachTagFromOrderAsync(donorOrder.Id, tag.Id);
                    await _orderTagRepository.Save();
                }
            }
        }

        foreach (var item in touchedOrderItems)
        {
            var pendingRemaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(item.Id);
            PendingReturnInvariantHelper.CheckOrderItemInvariant(item, pendingRemaining, _logger);
        }

        _logger.LogInformation($"Redistribution completed for urgent order {urgentOrder.Id}, {affectedOrders.Count} donor orders affected");
    }
}
