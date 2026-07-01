using Katino.Domain.Constants;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Helpers;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using Katino.Domain.Services.OrderN.SetOrderManualStatusService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class SetOrderManualStatusService : ISetOrderManualStatusService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDeleteOrderService _deleteOrderService;
    private readonly IOrderTagRepository _orderTagRepository;
    private readonly ILogger _logger;

    public SetOrderManualStatusService(
        IOrderRepository orderRepository,
        IDeleteOrderService deleteOrderService,
        IOrderTagRepository orderTagRepository,
        ILoggerFactory loggerFactory)
    {
        _orderRepository = orderRepository;
        _deleteOrderService = deleteOrderService;
        _orderTagRepository = orderTagRepository;
        _logger = loggerFactory?.CreateLogger(nameof(SetOrderManualStatusService));
    }

    public async Task<OrderStatus[]> GetNextOrderStatusesAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderWithOrderItemsAsync(orderId);

        if (order.DeliveryType == DeliveryType.NotNovaPost)
        {
            return GetNextOrderStatusesForNonNp(order.OrderStatus);
        }

        return GetNextOrderStatusesForNp(order.OrderStatus);
    }

    public async Task<bool> SetOrderManualStatusAsync(Guid orderId, OrderStatus orderStatus)
    {
        _logger.LogInformation($"Handling manual status change for order {orderId}: {orderStatus}");

        try
        {
            var order = await _orderRepository.GetOrderWithOrderItemsAsync(orderId);

            if (order.DeliveryType == DeliveryType.NotNovaPost)
            {
                OrderStatusHelper.ValidateManualOrderStatusForNonNp(orderStatus);
            }
            else
            {
                OrderStatusHelper.ValidateManualOrderStatus(orderStatus);
            }

            _logger.LogDebug($"Previous order status: {order.OrderStatus}, new status: {orderStatus}");
            var updateReason = $"Order {orderId} manual status change - from {order.OrderStatus} to {orderStatus}";

            if (orderStatus == OrderStatus.Refusal)
            {
                if (!InternetDocumentConstants.OrderReceivedStatuses.Contains(order.OrderStatus))
                {
                    throw new ArgumentException("Not yet received orders cannot be manually rejected");
                }

                order.OrderStatus = orderStatus;
                order.UpdatedAt = DateTimeOffset.UtcNow;
                order.UpdateReasonDetails = updateReason;

                // Status updated here
                await _deleteOrderService.HandleOrderRejectionAsync(order, null, false);

                _logger.LogTrace("Attaching RefundMoney tag to order");
                var refundTag = await _orderTagRepository.GetOrCreateByTypeAsync(OrderTagType.RefundMoney, canBeDeleted: true);
                if (!await _orderTagRepository.IsTagAttachedToOrderAsync(order.Id, refundTag.Id))
                {
                    await _orderTagRepository.AttachTagToOrderAsync(order.Id, refundTag.Id);
                    await _orderTagRepository.Save();
                }

                return true;
            }

            if (orderStatus == OrderStatus.Exchange)
            {
                if (InternetDocumentConstants.OrderRejectedStatuses.Contains(order.OrderStatus))
                {
                    order.OrderStatus = orderStatus;
                    order.UpdatedAt = DateTimeOffset.UtcNow;
                    order.UpdateReasonDetails = updateReason;

                    await _orderRepository.Save();

                    return true;
                }
                else if (InternetDocumentConstants.OrderReceivedStatuses.Contains(order.OrderStatus))
                {
                    order.OrderStatus = orderStatus;
                    order.UpdatedAt = DateTimeOffset.UtcNow;
                    order.UpdateReasonDetails = updateReason;

                    // Status updated here
                    await _deleteOrderService.HandleOrderRejectionAsync(order, null, true);

                    return true;
                }
                else
                {
                    throw new ArgumentException("Exchange can be set only for received or rejected statuses");
                }
            }

            if (orderStatus == OrderStatus.Received && order.DeliveryType == DeliveryType.NotNovaPost)
            {
                if (order.OrderStatus != OrderStatus.Packed)
                {
                    throw new ArgumentException("Received can only be set for Packed orders");
                }

                order.OrderStatus = orderStatus;
                order.UpdatedAt = DateTimeOffset.UtcNow;
                order.UpdateReasonDetails = updateReason;

                await _orderRepository.Save();

                return true;
            }

            if (orderStatus == OrderStatus.Packed)
            {
                if (order.OrderStatus != OrderStatus.ReadyToShip)
                {
                    throw new ArgumentException("Orders that are not ready to ship cannot be Packed");
                }

                order.OrderStatus = orderStatus;
                order.UpdatedAt = DateTimeOffset.UtcNow;
                order.UpdateReasonDetails = updateReason;

                var pendingIncomingReturnTag = await _orderTagRepository.GetOrCreateByTypeAsync(OrderTagType.PendingIncomingReturn, canBeDeleted: true);
                if (await _orderTagRepository.IsTagAttachedToOrderAsync(order.Id, pendingIncomingReturnTag.Id))
                {
                    _logger.LogTrace("Detaching PendingIncomingReturn tag from order");
                    await _orderTagRepository.DetachTagFromOrderAsync(order.Id, pendingIncomingReturnTag.Id);
                }

                await _orderRepository.Save();

                return true;
            }

            if (orderStatus == OrderStatus.ReadyToShip)
            {
                if (order.OrderStatus != OrderStatus.Packed)
                {
                    throw new ArgumentException("ReadyToShip statuses can be set only for Packed orders");
                }

                order.OrderStatus = orderStatus;
                order.UpdatedAt = DateTimeOffset.UtcNow;
                order.UpdateReasonDetails = updateReason;

                await _orderRepository.Save();

                return true;
            }

            _logger.LogInformation($"Manual status for order {orderId} was previously changed, it cannot be updated again");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while handling manual status change for order {orderId}");
            return false;
        }
    }

    private static OrderStatus[] GetNextOrderStatusesForNp(OrderStatus currentStatus)
    {
        if (currentStatus == OrderStatus.ReadyToShip)
        {
            return [OrderStatus.Packed];
        }

        if (currentStatus == OrderStatus.Packed)
        {
            return [OrderStatus.ReadyToShip];
        }

        if (InternetDocumentConstants.OrderReceivedStatuses.Contains(currentStatus))
        {
            return [OrderStatus.Refusal, OrderStatus.Exchange];
        }

        if (InternetDocumentConstants.OrderRejectedStatuses.Contains(currentStatus))
        {
            return [OrderStatus.Exchange];
        }

        return [];
    }

    private static OrderStatus[] GetNextOrderStatusesForNonNp(OrderStatus currentStatus)
    {
        if (currentStatus == OrderStatus.ReadyToShip)
        {
            return [OrderStatus.Packed];
        }

        if (currentStatus == OrderStatus.Packed)
        {
            return [OrderStatus.ReadyToShip, OrderStatus.Received];
        }

        if (InternetDocumentConstants.OrderReceivedStatuses.Contains(currentStatus))
        {
            return [OrderStatus.Refusal, OrderStatus.Exchange];
        }

        return [];
    }
}
