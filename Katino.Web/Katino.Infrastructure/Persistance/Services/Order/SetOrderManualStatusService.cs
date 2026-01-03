using Katino.Domain.Constants;
using Katino.Domain.Enums;
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

    public async Task<bool> SetOrderManualStatusAsync(Guid orderId, OrderManualStatus orderManualStatus)
    {
        _logger.LogInformation($"Handling manual status change for oder {orderId}");
        try
        {
            var order = await _orderRepository.GetOrderWithOrderItemsAsync(orderId);
            _logger.LogDebug($"Previous order manual status: {order.OrderManualStatus}, new status: {orderManualStatus}");

            if (order.OrderManualStatus == OrderManualStatus.None && orderManualStatus == OrderManualStatus.Refusal)
            {
                if (!InternetDocumentConstants.ReceivedStatuses.Contains(order.OrderInternetDocStatus))
                {
                    throw new ArgumentException("Not yet received orders cannot be manually rejected");
                }

                order.OrderManualStatus = orderManualStatus;

                // Status updated here
                await _deleteOrderService.HandleOrderRejectionAsync(order, null);

                return true;
            }

            if (order.OrderManualStatus == OrderManualStatus.None && orderManualStatus == OrderManualStatus.Exchange)
            {
                if (InternetDocumentConstants.RejectedStatuses.Contains(order.OrderInternetDocStatus))
                {
                    order.OrderManualStatus = orderManualStatus;

                    await _orderRepository.Save();

                    return true;
                }
                else if (InternetDocumentConstants.ReceivedStatuses.Contains(order.OrderInternetDocStatus))
                {
                    order.OrderManualStatus = orderManualStatus;

                    // Status updated here
                    await _deleteOrderService.HandleOrderRejectionAsync(order, null);

                    return true;
                }
                else
                {
                    throw new ArgumentException("Exchange can be set only for received or rejected statuses");
                }
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
