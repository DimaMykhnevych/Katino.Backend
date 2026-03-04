using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Domain.Repositories.FinanceEntryRepository;
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
    private readonly IFinanceEntryRepository _financeEntryRepository;
    private readonly IFinanceCategoryRepository _financeCategoryRepository;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly ILogger _logger;

    public DeleteOrderService(
        IOrderRepository orderRepository,
        IOrderAddressInfoRepository orderAddressInfoRepository,
        IProductVariantRepository productVariantRepository,
        IFinanceEntryRepository financeEntryRepository,
        IFinanceCategoryRepository financeCategoryRepository,
        IUpdateProductVariantService updateProductVariantService,
        IOrderItemChangeService orderItemChangeService,
        IInternetDocumentService internetDocumentService,
        ILoggerFactory loggerFactory)
    {
        _orderRepository = orderRepository;
        _orderAddressInfoRepository = orderAddressInfoRepository;
        _productVariantRepository = productVariantRepository;
        _financeEntryRepository = financeEntryRepository;
        _financeCategoryRepository = financeCategoryRepository;
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

            // Product variant statuses and possible other orders are updated during function app processing (or manual status change).
            // We don't need to separatly consider manual status, because manual status can only be set for
            // rejected or received int doc statuses, and if they have such statuses, it means that required
            // actions were already performed during function app processing (or manual status change).
            if (InternetDocumentConstants.OrderRejectedStatuses.Contains(existingOrder.OrderStatus) ||
                InternetDocumentConstants.OrderReceivedStatuses.Contains(existingOrder.OrderStatus) ||
                existingOrder.OrderStatus == OrderStatus.Refusal || existingOrder.OrderStatus == OrderStatus.Exchange)
            {
                _logger.LogInformation($"Order {id} is rejected/received, deleting only order and internet document");

                if (existingOrder.AddressInfo != null)
                {
                    _logger.LogDebug("Deleting order address info");
                    _orderAddressInfoRepository.Delete(existingOrder.AddressInfo);
                }

                // We don't need to subtract revenue during deletion of received orders.
                // In other cases the revenue should be already subtracted, it's just the double check,
                // because in TryReverseOrderFinanceAsync method there is appropriate check.
                if (!InternetDocumentConstants.OrderReceivedStatuses.Contains(existingOrder.OrderStatus))
                {
                    await TryReverseOrderFinanceAsync(
                        existingOrder,
                        FinanceEntryReason.OrderDeleted,
                        "Order deleted");
                }

                _orderRepository.Delete(existingOrder);
                await _orderRepository.Save();

                // Internet doc deletion
                var docDeletionResult = await DeleteInternetDocument(existingOrder);

                return new() { OrderDeletedSuccessfully = true, NpInternetDocDeletedSuccessfully = docDeletionResult };
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
                if (productVariant == null)
                {
                    continue;
                }

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

            await TryReverseOrderFinanceAsync(
                existingOrder,
                FinanceEntryReason.OrderDeleted,
                "Order deleted");

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

    public async Task HandleOrderRejectionAsync(Order order, OrderInternetDocStatus? orderInternetDocStatus, bool isManualExchange)
    {
        if (orderInternetDocStatus != null)
        {
            _logger.LogInformation($"Order {order.Id} was rejected with status {orderInternetDocStatus}, making return...");
        }
        else
        {
            _logger.LogInformation($"Order {order.Id} was manually rejected, making return...");
        }

        Dictionary<Guid, int> currentProductQuantities = [];
        Dictionary<Guid, int> productQuantitiesAfterProcessing = [];

        _logger.LogInformation($"[HandleOrderRejectionAsync] Handling order items return {order.Id}");
        await _orderItemChangeService.HandleOrderItemsReturn(order, currentProductQuantities, productQuantitiesAfterProcessing);

        var commentText = isManualExchange
            ? "Manual exchnage of already received order"
            : orderInternetDocStatus != null
            ? $"Rejected by NP: {orderInternetDocStatus}"
            : "Rejected manually";

        _logger.LogInformation($"Reversing order finance for {order.Id}");
        await TryReverseOrderFinanceAsync(
            order,
            FinanceEntryReason.OrderRefunded,
            commentText);

        await _orderRepository.Save();

        _logger.LogDebug("Updating current quantities");
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

    private async Task<bool> TryReverseOrderFinanceAsync(
        Order order,
        FinanceEntryReason reason,
        string comment = null)
    {
        var currentTotal = await _financeEntryRepository.GetOrderFinanceTotalAsync(order.Id);

        if (currentTotal == 0m)
        {
            return false;
        }

        var revenueCategory = await GetRevenueCategoryAsync();

        var ttn = !string.IsNullOrWhiteSpace(order.InternetDocumentIntDocNumber)
            ? order.InternetDocumentIntDocNumber
            : await _financeEntryRepository.GetAnyTtnByOrderIdAsync(order.Id);

        var reversal = new FinanceEntry
        {
            Id = Guid.NewGuid(),
            EntryDate = DateTimeHelper.GetCurrentKyivDateTime().Date,
            Amount = -currentTotal,
            Comment = comment,

            SourceType = FinanceEntrySourceType.Reversal,
            Reason = reason,

            SaleType = order.SaleType,
            IsLocked = true,
            InternetDocumentIntDocNumber = ttn,

            CategoryId = revenueCategory.Id,
            OrderId = order.Id,

            CreatedBy = null,
            ReversedEntryId = null,

            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _financeEntryRepository.Insert(reversal);

        return true;
    }

    private async Task<FinanceCategory> GetRevenueCategoryAsync()
    {
        var category = await _financeCategoryRepository
            .GetByTypeAndNameAsync(FinanceCategoryType.Income, FinanceCategoryNames.Revenue);

        if (category == null)
        {
            throw new InvalidOperationException("FinanceCategory 'Revenue' (Income) not found. Seed it or create it before using finance.");
        }

        return category;
    }
}
