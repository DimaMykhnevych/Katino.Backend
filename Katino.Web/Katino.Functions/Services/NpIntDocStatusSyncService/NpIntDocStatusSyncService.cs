using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
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
    private readonly ILogger _logger;

    public NpIntDocStatusSyncService(
        IInternetDocumentService internetDocumentService,
        IOrderRepository orderRepository,
        IProductVariantRepository productVariantRepository,
        IOrderItemChangeService orderItemChangeService,
        IUpdateProductVariantService updateProductVariantService,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _orderRepository = orderRepository;
        _productVariantRepository = productVariantRepository;
        _orderItemChangeService = orderItemChangeService;
        _updateProductVariantService = updateProductVariantService;
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
            //foreach (var item in order.OrderItems)
            //{
            //    item.Order = null;
            //}

            var orderStatusString = statusesDict[order.InternetDocumentIntDocNumber];
            var orderInternalDocStatus = (OrderInternetDocStatus)int.Parse(orderStatusString);

            // TODO remove----
            // TODO deal with entity with the same id is already tracked !!!
            //if(order.Id == Guid.Parse("08de45e9-822a-4f79-8ecb-321e72fe468c"))
            //{
            //    orderInternalDocStatus = OrderInternetDocStatus.Rejection;
            //}
            // ----

            order.OrderInternetDocStatus = orderInternalDocStatus;
            await _orderRepository.Update(order);

            // Handle rejected status
            if (InternetDocumentConstants.RejectedStatuses.Contains(orderInternalDocStatus))
            {
                _logger.LogInformation($"Order {order.Id} was rejected with status {orderInternalDocStatus}, making return...");

                Dictionary<Guid, int> currentProductQuantities = [];
                Dictionary<Guid, int> productQuantitiesAfterProcessing = [];

                await ProcessOrderItemsReturn(order, currentProductQuantities, productQuantitiesAfterProcessing);

                await _orderRepository.Save();

                foreach (var currentQuantity in currentProductQuantities)
                {
                    var updatedQuantity = productQuantitiesAfterProcessing[currentQuantity.Key];
                    if (updatedQuantity > currentQuantity.Value)
                    {
                        _logger.LogDebug($"Product variant quantity change detected (due to order rejection), product variant id: {currentQuantity.Key}, quantity: {updatedQuantity}");
                        await _updateProductVariantService
                            .HandleProductVariantQuantityChange(currentQuantity.Key, updatedQuantity, order.Id);
                    }
                }
            }
            else
            {
                await _orderRepository.Save();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during processing of order {order.Id}");
        }
    }

    private async Task ProcessOrderItemsReturn(
        Order order,
        Dictionary<Guid, int> currentProductQuantities,
        Dictionary<Guid, int> productQuantitiesAfterProcessing)
    {
        HashSet<Guid> currentOrderProductVariants = order.OrderItems
            .Select(x => x.ProductVariantId)
            .ToHashSet();

        _logger.LogDebug($"Getting product variants of order to reject");

        List<ProductVariant> productVariantsRelatedToCurrentOrder = [];
        foreach (var productVariantId in currentOrderProductVariants)
        {
            var productVariant = await _productVariantRepository.GetAsNoTracking(productVariantId);
            productVariantsRelatedToCurrentOrder.Add(productVariant);
            currentProductQuantities[productVariantId] = productVariant.QuantityInStock;
            productQuantitiesAfterProcessing[productVariantId] = productVariant.QuantityInStock;
        }

        _logger.LogDebug($"Handling rejected order items");
        await _orderItemChangeService
            .HandleDeletedOrderItems(order.SaleType, order.OrderItems, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder, false);
    }
}
