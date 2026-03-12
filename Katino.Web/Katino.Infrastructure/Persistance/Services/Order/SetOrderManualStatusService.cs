using Katino.Domain.Constants;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using Katino.Domain.Services.OrderN.SetOrderManualStatusService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class SetOrderManualStatusService : ISetOrderManualStatusService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDeleteOrderService _deleteOrderService;
    private readonly ILogger _logger;

    public SetOrderManualStatusService(
        IOrderRepository orderRepository,
        IDeleteOrderService deleteOrderService,
        ILoggerFactory loggerFactory)
    {
        _orderRepository = orderRepository;
        _deleteOrderService = deleteOrderService;
        _logger = loggerFactory?.CreateLogger(nameof(SetOrderManualStatusService));
    }

    public OrderStatus[] GetNextOrderStatuses(OrderStatus orderStatusCurrent)
    {
        if (orderStatusCurrent == OrderStatus.ReadyToShip)
        {
            return [OrderStatus.Packed];
        }

        if (orderStatusCurrent == OrderStatus.Packed)
        {
            return [OrderStatus.ReadyToShip];
        }

        if (InternetDocumentConstants.OrderReceivedStatuses.Contains(orderStatusCurrent))
        {
            return [OrderStatus.Refusal, OrderStatus.Exchange];
        }

        if (InternetDocumentConstants.OrderRejectedStatuses.Contains(orderStatusCurrent))
        {
            return [OrderStatus.Exchange];
        }

        return [];
    }

    public async Task<bool> SetOrderManualStatusAsync(Guid orderId, OrderStatus orderStatus)
    {
        _logger.LogInformation($"Handling manual status change for oder {orderId}: {orderStatus}");
        OrderStatusHelper.ValidateManualOrderStatus(orderStatus);

        try
        {
            var order = await _orderRepository.GetOrderWithOrderItemsAsync(orderId);
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

            if (orderStatus == OrderStatus.Packed)
            {
                if (order.OrderStatus != OrderStatus.ReadyToShip)
                {
                    throw new ArgumentException("Orders that are not ready to ship cannot be Packed");
                }

                order.OrderStatus = orderStatus;
                order.UpdatedAt = DateTimeOffset.UtcNow;
                order.UpdateReasonDetails = updateReason;

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
            _logger.LogError(ex, $"An error occurred while handling manual status change for oder {orderId}");
            return false;
        }
    }
}
