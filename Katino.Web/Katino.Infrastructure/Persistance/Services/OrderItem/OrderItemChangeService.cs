using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderItemN;

public class OrderItemChangeService : IOrderItemChangeService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly ILogger _logger;

    public OrderItemChangeService(
        IProductVariantRepository productVariantRepository,
        IOrderItemRepository orderItemRepository,
        IUpdateProductVariantService updateProductVariantService,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _orderItemRepository = orderItemRepository;
        _updateProductVariantService = updateProductVariantService;
        _logger = loggerFactory?.CreateLogger(nameof(OrderItemChangeService));
    }

    public async Task HandleAddedOrderItems(SaleType saleType, List<OrderItem> orderItems)
    {
        _logger.LogInformation($"Handling added order items ({orderItems.Count})");

        // Updating QuantityInStock, QuantityRegularSold and QuantityDropSold + ProductVariant status
        foreach (var orderItem in orderItems)
        {
            var productVariant = await _productVariantRepository.Get(orderItem.ProductVariantId);
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
                continue;
            }

            var newQuantityInStock = productVariant.QuantityInStock < orderItem.Quantity
                ? 0
                : productVariant.QuantityInStock - orderItem.Quantity;

            productVariant.QuantityInStock = newQuantityInStock;
            productVariant.Status = newQuantityInStock > 0 ? ProductStatus.InStock : ProductStatus.OnOrder;

            await _productVariantRepository.Update(productVariant);
        }

        await _productVariantRepository.Save();

        _logger.LogDebug("Order items product variants updated successfully");
    }

    public async Task HandleDeletedOrderItems(SaleType saleType, List<OrderItem> deletedItems)
    {
        _logger.LogInformation($"Handling deleted order items ({deletedItems.Count})");

        foreach (var orderItem in deletedItems)
        {
            _logger.LogTrace($"Processing deleted order item ({orderItem.Id}). Order id: {orderItem.OrderId}");

            var productVariant = await _productVariantRepository.GetAsNoTracking(orderItem.ProductVariantId);
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
                continue;
            }

            var oldProductVariantQuantityInStock = productVariant.QuantityInStock;

            if (orderItem.OrderItemStatus == OrderItemStatus.Ready)
            {
                productVariant.QuantityInStock += orderItem.Quantity;
            }
            else if (orderItem.OrderItemStatus == OrderItemStatus.ForSewing)
            {
                var actualAddedQuantity = orderItem.Quantity - orderItem.QuantityToProduce;
                productVariant.QuantityInStock += actualAddedQuantity;
            }

            productVariant.Status = productVariant.QuantityInStock > 0 ? ProductStatus.InStock : ProductStatus.OnOrder;

            bool hasQuantityChanged = oldProductVariantQuantityInStock != productVariant.QuantityInStock;

            // TODO currently save in db occurs during "await _orderRepository.Save();" in HandleProductVariantQuantityChange
            await _productVariantRepository.Update(productVariant);

            if (hasQuantityChanged)
            {
                _logger.LogDebug($"Product variant quantity change detected, product variant id: {orderItem.ProductVariantId}, quantity: {productVariant.QuantityInStock}");
                await _updateProductVariantService
                    .HandleProductVariantQuantityChange(orderItem.ProductVariantId, productVariant.QuantityInStock);
            }

            _orderItemRepository.Delete(orderItem);
        }

        await _productVariantRepository.Save();
        await _orderItemRepository.Save();

        _logger.LogDebug("Deleted order items was processed successfully");
    }

    public async Task ProcessCurrentOrderItemsStatuses(List<OrderItem> orderItems)
    {
        _logger.LogInformation($"Processing current order items statuses ({orderItems.Count})");

        foreach (var orderItem in orderItems)
        {
            if (orderItem.IsCustomTailoring)
            {
                orderItem.OrderItemStatus = OrderItemStatus.ForSewing;
                orderItem.QuantityToProduce = orderItem.Quantity;
                continue;
            }

            var productVariant = await _productVariantRepository.Get(orderItem.ProductVariantId);
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
}
