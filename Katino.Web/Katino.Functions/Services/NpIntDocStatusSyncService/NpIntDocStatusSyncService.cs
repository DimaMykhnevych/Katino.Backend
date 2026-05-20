using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Katino.Domain.Services.TelegramN;
using Microsoft.Extensions.Logging;

namespace Katino.Functions.Services.NpIntDocStatusSyncService;

public class NpIntDocStatusSyncService : INpIntDocStatusSyncService
{
    private const int OrderBatchSize = 10;

    private static readonly OrderInternetDocStatus[] ReceivedStatuses = [
        OrderInternetDocStatus.Received // 9
    ];

    private static readonly OrderInternetDocStatus[] RejectedStatuses = [
        OrderInternetDocStatus.RejectionBySender,           // 102
        OrderInternetDocStatus.Rejection,                   // 103
        OrderInternetDocStatus.StorageStopped,              // 105
        OrderInternetDocStatus.ReceivedAndReturnCreated,    // 106
        OrderInternetDocStatus.ReceiverNotAnswering         // 111
    ];

    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly IDeleteOrderService _deleteOrderService;
    private readonly IOrderTagRepository _orderTagRepository;
    private readonly IKatinoDbContext _dbContext;
    private readonly IOrderRejectionNotifier _orderRejectionNotifier;
    private readonly ILogger _logger;

    public NpIntDocStatusSyncService(
        IInternetDocumentService internetDocumentService,
        IOrderRepository orderRepository,
        IProductVariantRepository productVariantRepository,
        IOrderItemChangeService orderItemChangeService,
        IUpdateProductVariantService updateProductVariantService,
        IDeleteOrderService deleteOrderService,
        IOrderTagRepository orderTagRepository,
        IKatinoDbContext dbContext,
        IOrderRejectionNotifier orderRejectionNotifier,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _orderRepository = orderRepository;
        _productVariantRepository = productVariantRepository;
        _orderItemChangeService = orderItemChangeService;
        _updateProductVariantService = updateProductVariantService;
        _deleteOrderService = deleteOrderService;
        _orderTagRepository = orderTagRepository;
        _dbContext = dbContext;
        _orderRejectionNotifier = orderRejectionNotifier;
        _logger = loggerFactory?.CreateLogger(nameof(NpIntDocStatusSyncService));
    }

    public async Task RunSync()
    {
        List<OrderInternetDocStatus> excludedStatuses = [];
        excludedStatuses.AddRange(ReceivedStatuses);
        excludedStatuses.AddRange(RejectedStatuses);
        excludedStatuses.Add(OrderInternetDocStatus.Deleted);
        var ordersToCheck = await _orderRepository.GetOrdersForNpStatusUpdateAsync(excludedStatuses.ToArray());

        var batches = ordersToCheck.Chunk(OrderBatchSize);

        foreach (var batch in batches)
        {
            await ProccessOrderBatch(batch);
        }
    }

    private async Task ProccessOrderBatch(Order[] batch)
    {
        _logger.LogDebug("Processing order batch");

        try
        {
            var orderIntDocNumbers = batch.Select(o => o.InternetDocumentIntDocNumber).ToList();

            _logger.LogInformation($"Getting statuses for internet docs: {string.Join(",", orderIntDocNumbers)}");

            var statuses = await _internetDocumentService.GetIntDocStatuses(orderIntDocNumbers);
            var statusesDict = statuses.ToDictionary(k => k["Number"].ToString(), v => v["StatusCode"].ToString());
            foreach (var order in batch)
            {
                await ProcessOrder(order, statusesDict);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during batch processing");
        }
    }

    private async Task ProcessOrder(Order order, Dictionary<string, string> statusesDict)
    {
        _logger.LogDebug($"Processing order {order.Id}");
        try
        {
            var orderStatusString = statusesDict[order.InternetDocumentIntDocNumber];
            _logger.LogDebug($"Raw order status string: {orderStatusString}");

            var orderStatusNumber = int.Parse(orderStatusString);
            _logger.LogTrace($"Order status number: {orderStatusNumber}");

            if (!Enum.IsDefined(typeof(OrderInternetDocStatus), orderStatusNumber))
            {
                throw new ArgumentException($"Unknown NovaPost internet document status: {orderStatusNumber}", nameof(orderStatusNumber));
            }

            var orderInternetDocStatus = (OrderInternetDocStatus)orderStatusNumber;
            var orderStatus = GetOrderStatusFromIntDocStatus(orderInternetDocStatus);

            _logger.LogDebug($"Order with {order.Id} has InternetDocStatus {orderInternetDocStatus} and orderStatus {orderStatus}");
            bool shouldUpdateOrderStatus = OrderStatusHelper.ShouldUpdateToNpRelatedStatus(order.OrderStatus, orderStatus);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Handle rejected status
            if (RejectedStatuses.Contains(orderInternetDocStatus))
            {
                _logger.LogInformation($"Order with {order.Id} was rejected, handling rejection...");
                await _deleteOrderService.HandleOrderRejectionAsync(order, orderInternetDocStatus, false);

                if (order.AfterpaymentOnGoodsCost == null)
                {
                    _logger.LogTrace("Attaching RefundMoney tag to order {OrderId}", order.Id);
                    var refundTag = await _orderTagRepository.GetOrCreateByTypeAsync(OrderTagType.RefundMoney, canBeDeleted: true);
                    await _orderTagRepository.AttachTagToOrderAsync(order.Id, refundTag.Id);
                    await _orderTagRepository.Save();
                }
                else
                {
                    _logger.LogInformation("RefundMoney tag to order {OrderId} is not attached, it has AfterpaymentOnGoodsCost, {Cost}", order.Id, order.AfterpaymentOnGoodsCost);
                }

                try
                {
                    await _orderRejectionNotifier.NotifyAsync(order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send rejection notification for order {OrderId}", order.Id);
                }
            }
            else
            {
                _logger.LogTrace($"Order with {order.Id} wasn't rejected");
            }

            _logger.LogDebug($"Updating order with {order.Id}, shouldUpdateOrderStatus: {shouldUpdateOrderStatus}");
            await _orderRepository.UpdateInternetDocStatusAsync(order.Id, orderInternetDocStatus, orderStatus, shouldUpdateOrderStatus);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during processing of order {order.Id}");
        }
    }

    private OrderStatus GetOrderStatusFromIntDocStatus(OrderInternetDocStatus docStatus)
    {
        return docStatus switch
        {
            OrderInternetDocStatus.Created => OrderStatus.Created,
            OrderInternetDocStatus.Deleted => OrderStatus.Deleted,
            OrderInternetDocStatus.NotFound => OrderStatus.NotFound,
            OrderInternetDocStatus.InTheCityInterregional => OrderStatus.InTheCityInterregional,
            OrderInternetDocStatus.OnTheWayToCity => OrderStatus.OnTheWayToCity,
            OrderInternetDocStatus.OnTheWayToDepartment => OrderStatus.OnTheWayToDepartment,
            OrderInternetDocStatus.Arrived => OrderStatus.Arrived,
            OrderInternetDocStatus.ArrivedPostomat => OrderStatus.ArrivedPostomat,
            OrderInternetDocStatus.Received => OrderStatus.Received,
            OrderInternetDocStatus.ReceivedRemittancePending => OrderStatus.ReceivedRemittancePending,
            OrderInternetDocStatus.ReceivedRemittanceCompleted => OrderStatus.ReceivedRemittanceCompleted,
            OrderInternetDocStatus.NpCompletingOrder => OrderStatus.NpCompletingOrder,
            OrderInternetDocStatus.InTheCityWithinTheCity => OrderStatus.InTheCityWithinTheCity,
            OrderInternetDocStatus.OnTheWayToReceiver => OrderStatus.OnTheWayToReceiver,
            OrderInternetDocStatus.RejectionBySender => OrderStatus.RejectionBySender,
            OrderInternetDocStatus.Rejection => OrderStatus.Rejection,
            OrderInternetDocStatus.AddressChanged => OrderStatus.AddressChanged,
            OrderInternetDocStatus.StorageStopped => OrderStatus.StorageStopped,
            OrderInternetDocStatus.ReceivedAndReturnCreated => OrderStatus.ReceivedAndReturnCreated,
            OrderInternetDocStatus.ReceiverNotAnswering => OrderStatus.ReceiverNotAnswering,
            OrderInternetDocStatus.DeliveryDateChangedByReceiver => OrderStatus.DeliveryDateChangedByReceiver,
            _ => throw new ArgumentException($"Unknown internet doc status: {docStatus}", nameof(docStatus))
        };
    }
}
