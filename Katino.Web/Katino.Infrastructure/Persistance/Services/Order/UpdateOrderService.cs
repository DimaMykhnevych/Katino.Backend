using Katino.Domain.Constants;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Domain.Repositories.FinanceEntryRepository;
using Katino.Domain.Repositories.OrderAddressInfoRepository;
using Katino.Domain.Repositories.OrderNpOptionsSeatRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Katino.Domain.Services.OrderN.OrderDeliveryHandler;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.UpdateOrderService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class UpdateOrderService : IUpdateOrderService
{
    private readonly IAddNpCityService _addNpCityService;
    private readonly IAddNpContactPersonService _addNpContactPersonService;
    private readonly IOrderDeliveryHandlerFactory _deliveryHandlerFactory;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly IOrderAddressInfoRepository _orderAddressInfoRepository;
    private readonly IOrderNpOptionsSeatRepository _orderNpOptionsSeatRepository;
    private readonly IFinanceEntryRepository _financeEntryRepository;
    private readonly IFinanceCategoryRepository _financeCategoryRepository;
    private readonly ILogger _logger;

    public UpdateOrderService(
        IAddNpCityService addNpCityService,
        IAddNpContactPersonService addNpContactPersonService,
        IOrderDeliveryHandlerFactory deliveryHandlerFactory,
        IOrderRepository orderRepository,
        IKatinoDbContext katinoDbContext,
        IOrderItemChangeService orderItemChangeService,
        IProductVariantRepository productVariantRepository,
        IUpdateProductVariantService updateProductVariantService,
        IOrderAddressInfoRepository orderAddressInfoRepository,
        IOrderNpOptionsSeatRepository orderNpOptionsSeatRepository,
        IFinanceEntryRepository financeEntryRepository,
        IFinanceCategoryRepository financeCategoryRepository,
        ILoggerFactory loggerFactory)
    {
        _addNpCityService = addNpCityService;
        _addNpContactPersonService = addNpContactPersonService;
        _deliveryHandlerFactory = deliveryHandlerFactory;
        _orderRepository = orderRepository;
        _katinoDbContext = katinoDbContext;
        _orderItemChangeService = orderItemChangeService;
        _productVariantRepository = productVariantRepository;
        _updateProductVariantService = updateProductVariantService;
        _orderAddressInfoRepository = orderAddressInfoRepository;
        _orderNpOptionsSeatRepository = orderNpOptionsSeatRepository;
        _financeEntryRepository = financeEntryRepository;
        _financeCategoryRepository = financeCategoryRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateOrderService));
    }

    public async Task<OrderUpdateResult> UpdateAsync(Order order)
    {
        _logger.LogInformation($"Updating order, order items count: {order.OrderItems.Count}");
        try
        {
            var currentOrderInDb = await _orderRepository.GetExistingOrderForUpdate(order.Id);

            // We are doing this because in HandleDeletedOrderItems there is a call of _orderItemRepository.Delete(orderItem);.
            // Later in this method we call await _orderRepository.Update(updatedOrder); and the error thrown:
            // the entity with the same Id is already tracked. To resolve it we need to reset order properties of each order item.
            foreach (var item in currentOrderInDb.OrderItems)
            {
                item.Order = null;
            }

            // DeliveryType cannot be changed once set to NotNovaPost
            var effectiveDeliveryType = currentOrderInDb.DeliveryType == DeliveryType.NotNovaPost
                ? DeliveryType.NotNovaPost
                : order.DeliveryType;

            _logger.LogTrace($"Upserting sender city {order.SenderNpCity.Present}");
            var senderNpCityId = await _addNpCityService.UpsertNpCityAsync(order.SenderNpCity);

            Guid? recipientNpCityId = null;
            if (effectiveDeliveryType == DeliveryType.WarehouseOrPost)
            {
                _logger.LogTrace($"Upserting recipient city {order.RecipientNpCity.Present}");
                recipientNpCityId = await _addNpCityService.UpsertNpCityAsync(order.RecipientNpCity);
            }

            Guid senderContactPersonId = currentOrderInDb.SenderContactPersonId;
            if (order.SenderContactPerson.Ref != currentOrderInDb.SenderContactPerson.Ref ||
                order.SenderContactPerson.CounterpartyRef != currentOrderInDb.SenderContactPerson.CounterpartyRef)
            {
                _logger.LogTrace($"Upserting sender contact person {order.SenderContactPerson.Phones}");
                senderContactPersonId = await _addNpContactPersonService.UpsertNpContactPersonAsync(order.SenderContactPerson);
            }

            var deliveryHandler = _deliveryHandlerFactory.Create(effectiveDeliveryType);

            _logger.LogTrace("Resolving order recipient");
            var orderRecipientId = await deliveryHandler.ResolveOrderRecipientAsync(order);

            List<OrderNpOptionsSeat> npOptionSeats = await deliveryHandler.ResolveNpOptionSeatsAsync(order.OrderNpOptionsSeats);

            foreach (var orderOptionsSeat in currentOrderInDb.OrderNpOptionsSeats)
            {
                orderOptionsSeat.Order = null;
                _orderNpOptionsSeatRepository.Delete(orderOptionsSeat);
            }

            _logger.LogTrace("Processing current order items statuses");

            Dictionary<Guid, int> currentProductQuantities = [];
            Dictionary<Guid, int> productQuantitiesAfterProcessing = [];

            // ---------------------- Order processing and save ----------------------

            await using var transaction = await _katinoDbContext.Database.BeginTransactionAsync();

            Order updatedOrder;
            try
            {
                await HandleOrderItemsUpdate(order.OrderItems, currentOrderInDb.OrderItems, order.SaleType, currentProductQuantities, productQuantitiesAfterProcessing);

                // TODO all properties should be copied on update
                updatedOrder = new()
                {
                    Id = order.Id,
                    SenderNpWarehouseId = order.SenderNpWarehouseId,
                    RecipientNpWarehouseId = order.RecipientNpWarehouseId,
                    SenderNpCityId = senderNpCityId,
                    RecipientNpCityId = recipientNpCityId,
                    SenderContactPersonId = senderContactPersonId,
                    OrderRecipientId = orderRecipientId,
                    PayerType = order.PayerType,
                    PaymentMethod = order.PaymentMethod,
                    SaleType = currentOrderInDb.SaleType, // SaleType cannot be updated
                    CreationDateTime = currentOrderInDb.CreationDateTime,
                    SendUntilDate = order.SendUntilDate,
                    Weight = order.Weight,
                    DeliveryType = effectiveDeliveryType, // DeliveryType cannot be changed from NotNovaPost
                    SeatsAmount = order.SeatsAmount,
                    Description = order.Description,
                    Cost = order.Cost,
                    AfterpaymentOnGoodsCost = order.AfterpaymentOnGoodsCost,
                    OrderItems = order.OrderItems,
                    OrderNpOptionsSeats = npOptionSeats,
                    AddressInfo = order.AddressInfo,
                    InternetDocumentCreationAttempted = currentOrderInDb.InternetDocumentCreationAttempted,
                    InternetDocumentRef = currentOrderInDb.InternetDocumentRef,
                    InternetDocumentIntDocNumber = currentOrderInDb.InternetDocumentIntDocNumber,
                    OrderInternetDocStatus = currentOrderInDb.OrderInternetDocStatus,
                    OrderStatus = currentOrderInDb.OrderStatus,
                    Comment = order.Comment,
                    GeneralOrderInfo = order.GeneralOrderInfo,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdateReasonDetails = $"Manual order update." +
                    $" Previous orderItems: {string.Join(", ", currentOrderInDb.OrderItems.Select(oi => oi.ProductVariantId))}," +
                    $" orderItems new: {string.Join(", ", order.OrderItems.Select(oi => oi.ProductVariantId))}"
                };

                var newOrderStatus = order.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
                    ? OrderStatus.InProgress
                    : OrderStatus.ReadyToShip;

                var orderItemsChanged = OrderItemsChanged(currentOrderInDb.OrderItems, order.OrderItems);
                OrderStatusHelper.SetOrderStatus(updatedOrder, newOrderStatus, orderItemsChanged);

                _logger.LogTrace("Updating order in db");
                await _orderRepository.Update(updatedOrder);

                await ApplyOrderRevenueDeltaAsync(updatedOrder);

                // Handle deletion of AddressInfo
                if (order.AddressInfo == null && currentOrderInDb.AddressInfo != null)
                {
                    _orderAddressInfoRepository.Delete(currentOrderInDb.AddressInfo);
                    await _orderAddressInfoRepository.Save();
                }
                else
                {
                    await _orderRepository.Save();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during updating order");
                await transaction.RollbackAsync();
                throw;
            }

            // ---------------------- End Order processing and save ----------------------

            var npInternetDocUpdatedSuccessfully = await deliveryHandler.HandleInternetDocumentOnUpdateAsync(updatedOrder, currentOrderInDb);
            await UpdateFinanceEntriesTtnAsync(updatedOrder.Id, updatedOrder.InternetDocumentIntDocNumber);

            // At the end, after actual order update perform updates of other orders
            try
            {
                foreach (var currentQuantity in currentProductQuantities)
                {
                    var updatedQuantity = productQuantitiesAfterProcessing[currentQuantity.Key];
                    if (updatedQuantity > currentQuantity.Value)
                    {
                        _logger.LogDebug($"Product variant quantity change detected, product variant id: {currentQuantity.Key}, quantity: {updatedQuantity}");
                        await _updateProductVariantService
                            .HandleProductVariantQuantityChange(currentQuantity.Key, updatedQuantity, order.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during handling product variant quantity change");
            }

            return new() { OrderUpdatedSuccessfully = true, NpInternetDocUpdatedSuccessfully = npInternetDocUpdatedSuccessfully };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating order");
            return new();
        }
    }

    private async Task HandleOrderItemsUpdate(
        List<OrderItem> newOrderItems,
        List<OrderItem> existingOrderItemsFromDb,
        SaleType saleType,
        Dictionary<Guid, int> currentQuantities,
        Dictionary<Guid, int> productQuantitiesAfterProcessing)
    {
        _logger.LogTrace("Handling order items update");
        var addedOrderItems = newOrderItems.Where(i => i.Id == Guid.Empty).ToList();
        var existingOrderItems = newOrderItems.Where(i => i.Id != Guid.Empty).ToList();

        var existingItemIds = newOrderItems
            .Where(i => i.Id != Guid.Empty)
            .Select(i => i.Id)
            .ToHashSet();

        var deletedOrderItems = existingOrderItemsFromDb
            .Where(i => !existingItemIds.Contains(i.Id))
            .ToList();

        _logger.LogDebug(
            "Order items update: +{Added}, ~{Existing}, -{Deleted}",
            addedOrderItems.Count,
            existingItemIds.Count,
            deletedOrderItems.Count);

        var newProductVariantIds = newOrderItems.Select(i => i.ProductVariantId);
        var existingProductVariantIds = existingOrderItemsFromDb.Select(i => i.ProductVariantId);
        HashSet<Guid> currentOrderProductVariants = newProductVariantIds
            .Union(existingProductVariantIds)
            .ToHashSet();

        List<ProductVariant> productVariantsRelatedToCurrentOrder = [];
        foreach (var productVariantId in currentOrderProductVariants)
        {
            var productVariant = await _productVariantRepository.GetAsNoTracking(productVariantId);
            if (productVariant == null)
            {
                throw new ArgumentException($"Product variant with Id is deleted: {productVariantId}");
            }

            productVariantsRelatedToCurrentOrder.Add(productVariant);
            currentQuantities[productVariantId] = productVariant.QuantityInStock;
            productQuantitiesAfterProcessing[productVariantId] = productVariant.QuantityInStock;
        }

        // The ordering of processing of order items is important!
        // Firstly - HandleDeletedOrderItems, because some quantities may be added.
        // Secondly - HandleUpdatedOrderItems, because some quantities may be added or subtracted. Here the existing order item statuses are updated.
        // Finally - new order items, quantities only can be subtracted

        await _orderItemChangeService.HandleDeletedOrderItems(saleType, deletedOrderItems, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder);
        await _orderItemChangeService.HandleUpdatedOrderItems(saleType, existingOrderItems, existingOrderItemsFromDb, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder);

        // ProcessNewOrderItemsStatuses should be called before HandleAddedOrderItems, because in
        // HandleAddedOrderItems product variant quantities are changed and because of that
        // there may be incorrect statuses, if we call ProcessNewOrderItemsStatuses after HandleAddedOrderItems
        _orderItemChangeService.ProcessNewOrderItemsStatuses(addedOrderItems, productVariantsRelatedToCurrentOrder);
        await _orderItemChangeService.HandleAddedOrderItems(saleType, addedOrderItems, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder);
    }

    private async Task ApplyOrderRevenueDeltaAsync(Order updatedOrder)
    {
        var desiredTotal = Convert.ToDecimal(updatedOrder.Cost);
        var existingTotal = await _financeEntryRepository.GetOrderFinanceTotalAsync(updatedOrder.Id);

        var delta = desiredTotal - existingTotal;
        if (delta == 0m)
        {
            return;
        }

        var revenueCategory = await GetRevenueCategoryAsync();
        var hasAny = await _financeEntryRepository.AnyByOrderIdAsync(updatedOrder.Id);

        var entry = new FinanceEntry
        {
            Id = Guid.NewGuid(),
            EntryDate = DateTimeHelper.GetCurrentKyivDateTime().Date,
            Amount = delta,
            Comment = null,

            SourceType = hasAny ? FinanceEntrySourceType.Adjustment : FinanceEntrySourceType.Order,
            Reason = FinanceEntryReason.None,

            SaleType = updatedOrder.SaleType,
            IsLocked = true,
            InternetDocumentIntDocNumber = updatedOrder.InternetDocumentIntDocNumber,

            CategoryId = revenueCategory.Id,

            OrderId = updatedOrder.Id,

            CreatedBy = null,
            ReversedEntryId = null,

            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _financeEntryRepository.Insert(entry);
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

    private async Task UpdateFinanceEntriesTtnAsync(Guid orderId, string? ttn)
    {
        if (string.IsNullOrWhiteSpace(ttn))
        {
            return;
        }

        var entries = await _financeEntryRepository.GetByOrderIdAsync(orderId);
        var changed = false;

        foreach (var e in entries)
        {
            if (e.InternetDocumentIntDocNumber != ttn)
            {
                e.InternetDocumentIntDocNumber = ttn;
                e.UpdatedAtUtc = DateTime.UtcNow;
                changed = true;
            }
        }

        if (changed)
        {
            await _financeEntryRepository.Save();
        }
    }

    private bool OrderItemsChanged(List<OrderItem> existingOrderItems, List<OrderItem> newOrderItems)
    {
        if (existingOrderItems.Count != newOrderItems.Count || newOrderItems.Any(o => o.Id == Guid.Empty))
        {
            return true;
        }

        foreach (var existingOrderItem in existingOrderItems)
        {
            var correspondingNewOrderItem = newOrderItems.FirstOrDefault(i => i.Id == existingOrderItem.Id);
            if (correspondingNewOrderItem == null)
            {
                return true;
            }

            if (existingOrderItem.IsCustomTailoring != correspondingNewOrderItem.IsCustomTailoring ||
                existingOrderItem.Comment != correspondingNewOrderItem.Comment ||
                existingOrderItem.Quantity != correspondingNewOrderItem.Quantity ||
                existingOrderItem.ProductVariantId != correspondingNewOrderItem.ProductVariantId)
            {
                return true;
            }
        }

        return false;
    }
}
