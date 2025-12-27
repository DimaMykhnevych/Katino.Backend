using Katino.Domain.Entities;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderAddressInfoRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class DeleteOrderService : IDeleteOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderAddressInfoRepository _orderAddressInfoRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly ILogger _logger;

    public DeleteOrderService(
        IOrderRepository orderRepository,
        IOrderAddressInfoRepository orderAddressInfoRepository,
        IProductVariantRepository productVariantRepository,
        IUpdateProductVariantService updateProductVariantService,
        IOrderItemChangeService orderItemChangeService,
        IInternetDocumentService internetDocumentService,
        ILoggerFactory loggerFactory)
    {
        _orderRepository = orderRepository;
        _orderAddressInfoRepository = orderAddressInfoRepository;
        _productVariantRepository = productVariantRepository;
        _updateProductVariantService = updateProductVariantService;
        _orderItemChangeService = orderItemChangeService;
        _internetDocumentService = internetDocumentService;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteOrderService));
    }

    public async Task<OrderDeleteResult> DeleteAsync(Guid id)
    {
        _logger.LogInformation($"Deleting order, order id: {id}");

        try
        {
            var existingOrder = await _orderRepository.GetExistingOrderForDelete(id);
            if (existingOrder == null)
            {
                throw new ArgumentException($"Order with id {id} doesn't exist");
            }

            // We are doing this because in HandleDeletedOrderItems there is a call of _orderItemRepository.Delete(orderItem);.
            // Later in this method we call await _orderRepository.Update(updatedOrder); ant the error thrown:
            // the entity with the same Id is already tracked. To reslove it we need to reset order properties of each order item.
            foreach (var item in existingOrder.OrderItems)
            {
                item.Order = null;
            }

            Dictionary<Guid, int> currentProductQuantities = [];
            Dictionary<Guid, int> productQuantitiesAfterProcessing = [];

            HashSet<Guid> currentOrderProductVariants = existingOrder.OrderItems
                .Select(x => x.ProductVariantId)
                .ToHashSet();

            _logger.LogDebug($"Getting product variants of order to delete");

            List<ProductVariant> productVariantsRelatedToCurrentOrder = [];
            foreach (var productVariantId in currentOrderProductVariants)
            {
                var productVariant = await _productVariantRepository.GetAsNoTracking(productVariantId);
                productVariantsRelatedToCurrentOrder.Add(productVariant);
                currentProductQuantities[productVariantId] = productVariant.QuantityInStock;
                productQuantitiesAfterProcessing[productVariantId] = productVariant.QuantityInStock;
            }

            _logger.LogDebug($"Handling deleted order items");
            await _orderItemChangeService
                .HandleDeletedOrderItems(existingOrder.SaleType, existingOrder.OrderItems, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder);

            if (existingOrder.AddressInfo != null)
            {
                _logger.LogDebug($"Deleting order address info");
                _orderAddressInfoRepository.Delete(existingOrder.AddressInfo);
            }

            _orderRepository.Delete(existingOrder);
            await _orderRepository.Save();

            // Internet doc deletion
            var npInternetDocDeletedSuccessfully = await DeleteInternetDocument(existingOrder);

            // At the end, after actual order deletion perform updates of other orders
            try
            {
                foreach (var currentQuantity in currentProductQuantities)
                {
                    var updatedQuantity = productQuantitiesAfterProcessing[currentQuantity.Key];
                    if (updatedQuantity > currentQuantity.Value)
                    {
                        _logger.LogDebug($"Product variant quantity change detected (due to order deletion), product variant id: {currentQuantity.Key}, quantity: {updatedQuantity}");
                        await _updateProductVariantService
                            .HandleProductVariantQuantityChange(currentQuantity.Key, updatedQuantity, id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during handling prduct variant quantity change (in result of order delete)");
            }

            return new() { OrderDeletedSuccessfully = true, NpInternetDocDeletedSuccessfully = npInternetDocDeletedSuccessfully };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting order {id}");
            return new();
        }
    }

    private async Task<bool> DeleteInternetDocument(Order currentOrderInDb)
    {
        try
        {
            if (string.IsNullOrEmpty(currentOrderInDb.InternetDocumentRef))
            {
                _logger.LogInformation($"Order {currentOrderInDb.Id} dosen't have associated internet document, skipping deletion of it");
                return true;
            }

            _logger.LogInformation($"Deleting internet document for order {currentOrderInDb.Id}");

            return await _internetDocumentService.DeleteInternetDocumentAsync(currentOrderInDb.InternetDocumentRef);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting NP internet document for order");
            return false;
        }
    }
}
