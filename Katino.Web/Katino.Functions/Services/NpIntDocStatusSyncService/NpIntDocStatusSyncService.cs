using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.Extensions.Logging;

namespace Katino.Functions.Services.NpIntDocStatusSyncService;

public class NpIntDocStatusSyncService : INpIntDocStatusSyncService
{
    private const int OrderBatchSize = 10;

    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly IDeleteOrderService _deleteOrderService;
    private readonly ILogger _logger;

    public NpIntDocStatusSyncService(
        IInternetDocumentService internetDocumentService,
        IOrderRepository orderRepository,
        IProductVariantRepository productVariantRepository,
        IOrderItemChangeService orderItemChangeService,
        IUpdateProductVariantService updateProductVariantService,
        IDeleteOrderService deleteOrderService,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _orderRepository = orderRepository;
        _productVariantRepository = productVariantRepository;
        _orderItemChangeService = orderItemChangeService;
        _updateProductVariantService = updateProductVariantService;
        _deleteOrderService = deleteOrderService;
        _logger = loggerFactory?.CreateLogger(nameof(NpIntDocStatusSyncService));
    }

    public async Task RunSync()
    {
        List<OrderInternetDocStatus> excludedStatuses = [];
        excludedStatuses.AddRange(InternetDocumentConstants.ReceivedStatuses);
        excludedStatuses.AddRange(InternetDocumentConstants.RejectedStatuses);
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

            _logger.LogTrace($"Getting statuses for internet docs: {string.Join(",", orderIntDocNumbers)}");

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
            var orderInternetDocStatus = (OrderInternetDocStatus)int.Parse(orderStatusString);

            await _orderRepository.UpdateInternetDocStatusAsync(order.Id, orderInternetDocStatus);

            // Handle rejected status
            if (InternetDocumentConstants.RejectedStatuses.Contains(orderInternetDocStatus))
            {
                await _deleteOrderService.HandleOrderRejectionAsync(order, orderInternetDocStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during processing of order {order.Id}");
        }
    }
}
