using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderItemN;

public class OrderItemChangeService : IOrderItemChangeService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductVariantRedistributionHistoryRepository _redistributionHistoryRepository;
    private readonly ILogger _logger;

    public OrderItemChangeService(
        IProductVariantRepository productVariantRepository,
        IOrderItemRepository orderItemRepository,
        IProductVariantRedistributionHistoryRepository redistributionHistoryRepository,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _orderItemRepository = orderItemRepository;
        _redistributionHistoryRepository = redistributionHistoryRepository;
        _logger = loggerFactory?.CreateLogger(nameof(OrderItemChangeService));
    }

    public async Task HandleAddedOrderItems(
        SaleType saleType,
        List<OrderItem> orderItems,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        List<ProductVariant> productVariants)
    {
        _logger.LogInformation($"Handling added order items ({orderItems.Count})");

        // Updating QuantityInStock, QuantityRegularSold and QuantityDropSold + ProductVariant status
        foreach (var orderItem in orderItems)
        {
            var productVariant = productVariants.First(pv => pv.Id == orderItem.ProductVariantId);
            await ProcessOrderItemProductVariantAddition(productVariant, saleType, orderItem, productQuantitiesAfterProcessing);
        }

        _logger.LogDebug("Order items product variants updated successfully");
    }

    public async Task HandleDeletedOrderItems(
        SaleType saleType,
        List<OrderItem> deletedItems,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        Dictionary<Guid, int> phantomQuantitiesFreed,
        List<ProductVariant> productVariants,
        bool deleteOrderItems = true)
    {
        if (!deletedItems.Any())
        {
            return;
        }

        _logger.LogInformation($"Handling deleted order items ({deletedItems.Count})");

        foreach (var orderItem in deletedItems)
        {
            _logger.LogTrace($"Processing deleted order item ({orderItem.Id}). Order id: {orderItem.OrderId}");

            var productVariant = productVariants.FirstOrDefault(pv => pv.Id == orderItem.ProductVariantId);
            if (productVariant == null)
            {
                _logger.LogWarning($"Product variant is deleted: {orderItem.ProductVariantId}");
                continue;
            }

            await ProcessOrderItemProductVariantDeletion(productVariant, saleType, orderItem, productQuantitiesAfterProcessing, phantomQuantitiesFreed);

            if (deleteOrderItems)
            {
                _orderItemRepository.Delete(orderItem);
            }
        }

        _logger.LogDebug("Deleted order items was processed successfully");
    }

    public async Task HandleUpdatedOrderItems(
        SaleType saleType,
        List<OrderItem> itemsToUpdate,
        List<OrderItem> existingOrderItemsFromDb,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        Dictionary<Guid, int> phantomQuantitiesFreed,
        List<ProductVariant> productVariants)
    {
        _logger.LogInformation($"Handling updated order items ({itemsToUpdate.Count})");

        foreach (var item in itemsToUpdate)
        {
            var existingOrderItem = existingOrderItemsFromDb.FirstOrDefault(i => i.Id == item.Id);
            var productVariantNew = productVariants.First(pv => pv.Id == item.ProductVariantId);
            var productVariantExisting = productVariants.First(pv => pv.Id == existingOrderItem.ProductVariantId);

            // The item survives under the same Id if the product variant didn't change - only then should
            // any leftover pending-return coverage stay attached to it instead of being released whole.
            var survivingQuantity = item.ProductVariantId == existingOrderItem.ProductVariantId ? item.Quantity : 0;

            // In ProcessOrderItemProductVariantDeletion there may be addtion of product variant quantities
            await ProcessOrderItemProductVariantDeletion(productVariantExisting, saleType, existingOrderItem, productQuantitiesAfterProcessing, phantomQuantitiesFreed, survivingQuantity);

            // We calculate the status considering added quantities
            ProcessExistingOrderItemsStatus(item, existingOrderItem, productVariantNew);

            var pendingRemaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(item.Id);
            PendingReturnInvariantHelper.CheckOrderItemInvariant(item, pendingRemaining, _logger);

            // Now decreasing quantities
            await ProcessOrderItemProductVariantAddition(productVariantNew, saleType, item, productQuantitiesAfterProcessing);

            _logger.LogDebug("Order items updated successfully");
        }
    }

    public async Task HandleOrderItemsReturn(
        Order order,
        Dictionary<Guid, int> currentProductQuantities,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        Dictionary<Guid, int> phantomQuantitiesFreed)
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
        await HandleDeletedOrderItems(order.SaleType, order.OrderItems, productQuantitiesAfterProcessing, phantomQuantitiesFreed, productVariantsRelatedToCurrentOrder, false);
    }

    public void ProcessExistingOrderItemsStatus(
        OrderItem orderItem,
        OrderItem previouslyExistedOrderItem,
        ProductVariant productVariant)
    {
        _logger.LogInformation($"Processing existing order item status ({orderItem.Id})");

        // Same custom parameters
        if (orderItem.IsCustomTailoring && orderItem.Comment.Equals(previouslyExistedOrderItem.Comment, StringComparison.InvariantCultureIgnoreCase))
        {
            if (previouslyExistedOrderItem.OrderItemStatus == OrderItemStatus.Ready &&
                previouslyExistedOrderItem.Quantity > orderItem.Quantity)
            {
                orderItem.OrderItemStatus = OrderItemStatus.Ready;
                orderItem.QuantityToProduce = 0;
                return;
            }

            if (previouslyExistedOrderItem.OrderItemStatus == OrderItemStatus.Ready &&
                previouslyExistedOrderItem.Quantity < orderItem.Quantity)
            {
                orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
                orderItem.QuantityToProduce = orderItem.Quantity - previouslyExistedOrderItem.Quantity;
                return;
            }

            if (previouslyExistedOrderItem.OrderItemStatus == OrderItemStatus.ForSewing)
            {
                var readyAmount = previouslyExistedOrderItem.Quantity - previouslyExistedOrderItem.QuantityToProduce;
                var newQuatntityToProduce = readyAmount < orderItem.Quantity ?
                    orderItem.Quantity - readyAmount :
                    0;

                orderItem.OrderItemStatus = newQuatntityToProduce > 0 ? OrderItemStatus.ForSewing : OrderItemStatus.Ready;
                orderItem.QuantityToProduce = newQuatntityToProduce;
                return;
            }
        }
        else if (orderItem.IsCustomTailoring)
        {
            orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
            orderItem.QuantityToProduce = orderItem.Quantity;
            return;
        }

        if (productVariant.Status == ProductStatus.OnOrder)
        {
            orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
            orderItem.QuantityToProduce = orderItem.Quantity;
            return;
        }

        if (productVariant.Status == ProductStatus.Discontinued)
        {
            return;
        }

        if (productVariant.Status == ProductStatus.InStock)
        {
            var enoughItemsInStock = productVariant.QuantityInStock - orderItem.Quantity >= 0;
            if (enoughItemsInStock)
            {
                orderItem.OrderItemStatus = OrderItemStatus.Ready;
                orderItem.QuantityToProduce = 0;
            }
            else
            {
                orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
                orderItem.QuantityToProduce = orderItem.Quantity - productVariant.QuantityInStock;
            }
        }

        _logger.LogDebug("Order item statuses processed successfully");
    }

    public void ProcessNewOrderItemsStatuses(
        List<OrderItem> orderItems,
        List<ProductVariant> relatedProductVariants)
    {
        _logger.LogInformation($"Processing current order items statuses ({orderItems.Count})");

        foreach (var orderItem in orderItems)
        {
            // It is existing order item
            if (orderItem.Id != Guid.Empty)
            {
                // OrderItemStatus and QuantityToProduce for existing items are calculated in HandleUpdatedOrderItems (ProcessExistingOrderItemsStatus)
                continue;
            }

            if (orderItem.IsCustomTailoring)
            {
                orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
                orderItem.QuantityToProduce = orderItem.Quantity;
                continue;
            }

            var productVariant = relatedProductVariants.First(pv => pv.Id == orderItem.ProductVariantId);
            if (productVariant.Status == ProductStatus.OnOrder)
            {
                orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
                orderItem.QuantityToProduce = orderItem.Quantity;
                continue;
            }

            if (productVariant.Status == ProductStatus.Discontinued)
            {
                continue;
            }

            if (productVariant.Status == ProductStatus.InStock)
            {
                var enoughItemsInStock = productVariant.QuantityInStock - orderItem.Quantity >= 0;
                if (enoughItemsInStock)
                {
                    orderItem.OrderItemStatus = OrderItemStatus.Ready;
                    orderItem.QuantityToProduce = 0;
                }
                else
                {
                    orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
                    orderItem.QuantityToProduce = orderItem.Quantity - productVariant.QuantityInStock;
                }
            }
        }

        _logger.LogDebug("Orde item statuses processed successfully");
    }

    private async Task ProcessOrderItemProductVariantAddition(
        ProductVariant productVariant,
        SaleType saleType,
        OrderItem orderItem,
        Dictionary<Guid, int> productQuantitiesAfterProcessing)
    {
        if (saleType == SaleType.Retail)
        {
            productVariant.QuantityRegularSold += orderItem.Quantity;
        }
        else if (saleType == SaleType.Drop || saleType == SaleType.Wholesale)
        {
            productVariant.QuantityDropSold += orderItem.Quantity;
        }

        if (orderItem.IsCustomTailoring || productVariant.Status != ProductStatus.InStock)
        {
            await _productVariantRepository.Update(productVariant);
            productQuantitiesAfterProcessing[productVariant.Id] = productVariant.QuantityInStock;
            return;
        }

        var newQuantityInStock = productVariant.QuantityInStock < orderItem.Quantity
            ? 0
            : productVariant.QuantityInStock - orderItem.Quantity;

        productVariant.QuantityInStock = newQuantityInStock;
        productVariant.Status = newQuantityInStock > 0 ? ProductStatus.InStock : ProductStatus.OnOrder;

        await _productVariantRepository.Update(productVariant);
        productQuantitiesAfterProcessing[productVariant.Id] = productVariant.QuantityInStock;
    }

    private async Task ProcessOrderItemProductVariantDeletion(
        ProductVariant productVariant,
        SaleType saleType,
        OrderItem orderItem,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        Dictionary<Guid, int> phantomQuantitiesFreed,
        int survivingQuantity = 0)
    {
        if (saleType == SaleType.Retail)
        {
            productVariant.QuantityRegularSold -= orderItem.Quantity;
        }
        else if (saleType == SaleType.Drop || saleType == SaleType.Wholesale)
        {
            productVariant.QuantityDropSold -= orderItem.Quantity;
        }

        // Ignore custom tailoring in product variant QuantityInStock and Status
        if (orderItem.IsCustomTailoring)
        {
            await _productVariantRepository.Update(productVariant);
            productQuantitiesAfterProcessing[productVariant.Id] = productVariant.QuantityInStock;
            return;
        }

        var freedQuantity = orderItem.OrderItemStatus switch
        {
            OrderItemStatus.Ready => orderItem.Quantity,
            OrderItemStatus.ForSewing => orderItem.Quantity - orderItem.QuantityToProduce,
            _ => 0
        };

        if (freedQuantity > 0)
        {
            // Some (or all) of this item's coverage may itself still be an unresolved pending return
            // (this order became Ready/ForSewing-partial via an earlier redistribution that hasn't
            // physically arrived yet). That part must travel with the freed quantity as phantom, not
            // be treated as genuinely available stock.
            //
            // When called for an update (survivingQuantity > 0) rather than a real removal, only release
            // pending coverage for the portion of the reduction that actually eats into what was covered
            // (freedQuantity) - not the raw Quantity delta. The uncovered part of the old quantity
            // (QuantityToProduce) has nothing to release in the first place; shrinking it just means less
            // production is needed, it never touches the pending ledger. Using the raw Quantity delta here
            // would release pending coverage that should have stayed untouched whenever the item already
            // had some genuinely-uncovered portion before the edit.
            var phantomRemaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(orderItem.Id);
            var coverageReduction = Math.Max(0, freedQuantity - survivingQuantity);
            var phantomPart = Math.Min(coverageReduction, phantomRemaining);
            if (phantomPart > 0)
            {
                await _redistributionHistoryRepository.ConsumePendingReturnAsync(orderItem.Id, phantomPart);
                phantomQuantitiesFreed[productVariant.Id] = phantomQuantitiesFreed.GetValueOrDefault(productVariant.Id) + phantomPart;
            }

            productVariant.QuantityInStock += freedQuantity;
        }

        productVariant.Status = productVariant.QuantityInStock > 0 ? ProductStatus.InStock : ProductStatus.OnOrder;
        await _productVariantRepository.Update(productVariant);

        productQuantitiesAfterProcessing[productVariant.Id] = productVariant.QuantityInStock;
    }
}
