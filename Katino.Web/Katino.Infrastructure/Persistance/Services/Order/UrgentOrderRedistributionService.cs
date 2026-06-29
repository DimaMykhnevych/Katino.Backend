using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.OrderN.UrgentOrderRedistributionService;
using Katino.Domain.Services.ProductVariantRedistributionN.ProductVariantRedistributionRecorder;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class UrgentOrderRedistributionService : IUrgentOrderRedistributionService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductVariantRedistributionRecorder _redistributionRecorder;
    private readonly ILogger _logger;

    public UrgentOrderRedistributionService(
        IOrderRepository orderRepository,
        IProductVariantRedistributionRecorder redistributionRecorder,
        ILoggerFactory loggerFactory)
    {
        _orderRepository = orderRepository;
        _redistributionRecorder = redistributionRecorder;
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
        var redistributionEvents = new List<ProductVariantRedistributionEvent>();

        foreach (var urgentItem in forSewingItems)
        {
            var remainingToFill = urgentItem.QuantityToProduce;
            if (remainingToFill <= 0) continue;

            var lessUrgentOrders = await _orderRepository
                .GetLessUrgentOrdersWithProductVariantAsync(urgentItem.ProductVariantId, urgentOrder.SendUntilDate);

            foreach (var donorOrder in lessUrgentOrders)
            {
                if (remainingToFill <= 0) break;

                var donorItem = donorOrder.OrderItems
                    .FirstOrDefault(i => i.ProductVariantId == urgentItem.ProductVariantId && !i.IsCustomTailoring);

                if (donorItem == null) continue;

                var readyQuantity = donorItem.Quantity - donorItem.QuantityToProduce;
                if (readyQuantity <= 0) continue;

                var quantityToTake = Math.Min(readyQuantity, remainingToFill);

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

                redistributionEvents.Add(new ProductVariantRedistributionEvent
                {
                    ProductVariantId = urgentItem.ProductVariantId,
                    Reason = ProductVariantQuantityChangeReason.UrgentReallocation,
                    SourceOrderId = donorOrder.Id,
                    SourceOrderItemId = donorItem.Id,
                    SourceOrderTtnSnapshot = donorOrder.InternetDocumentIntDocNumber,
                    Lines = [
                        new ProductVariantRedistributionLine
                        {
                            TargetOrderId = urgentOrder.Id,
                            TargetOrderItemId = urgentItem.Id,
                            Quantity = quantityToTake
                        }
                    ]
                });

                _logger.LogDebug($"Redistributed {quantityToTake} units of product variant {urgentItem.ProductVariantId} from order {donorOrder.Id} to urgent order {urgentOrder.Id}");
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

        _logger.LogInformation($"Redistribution completed for urgent order {urgentOrder.Id}, {affectedOrders.Count} donor orders affected");
    }
}
