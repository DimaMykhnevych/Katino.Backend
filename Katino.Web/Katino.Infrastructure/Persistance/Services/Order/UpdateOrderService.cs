using Katino.Domain.Constants;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Repositories.OrderAddressInfoRepository;
using Katino.Domain.Repositories.OrderNpOptionsSeatRepository;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.UpdateOrderService;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class UpdateOrderService : IUpdateOrderService
{
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IAddNpCityService _addNpCityService;
    private readonly IAddNpContactPersonService _addNpContactPersonService;
    private readonly IAddOrderRecipientService _addOrderRecipientService;
    private readonly IContactPersonService _contactPersonService;
    private readonly IOrderRecipientRepository _orderRecipientRepository;
    private readonly IAddNpOptionsSeatService _addNpOptionsSeatService;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly IOrderAddressInfoRepository _orderAddressInfoRepository;
    private readonly IOrderNpOptionsSeatRepository _orderNpOptionsSeatRepository;
    private readonly ILogger _logger;

    public UpdateOrderService(
        IInternetDocumentService internetDocumentService,
        IAddNpCityService addNpCityService,
        IAddNpContactPersonService addNpContactPersonService,
        IAddOrderRecipientService addOrderRecipientService,
        IContactPersonService contactPersonService,
        IOrderRecipientRepository orderRecipientRepository,
        IAddNpOptionsSeatService addNpOptionsSeatService,
        IOrderRepository orderRepository,
        IKatinoDbContext katinoDbContext,
        IOrderItemChangeService orderItemChangeService,
        IProductVariantRepository productVariantRepository,
        IUpdateProductVariantService updateProductVariantService,
        IOrderAddressInfoRepository orderAddressInfoRepository,
        IOrderNpOptionsSeatRepository orderNpOptionsSeatRepository,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _addNpCityService = addNpCityService;
        _addNpContactPersonService = addNpContactPersonService;
        _addOrderRecipientService = addOrderRecipientService;
        _contactPersonService = contactPersonService;
        _orderRecipientRepository = orderRecipientRepository;
        _addNpOptionsSeatService = addNpOptionsSeatService;
        _orderRepository = orderRepository;
        _katinoDbContext = katinoDbContext;
        _orderItemChangeService = orderItemChangeService;
        _productVariantRepository = productVariantRepository;
        _updateProductVariantService = updateProductVariantService;
        _orderAddressInfoRepository = orderAddressInfoRepository;
        _orderNpOptionsSeatRepository = orderNpOptionsSeatRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateOrderService));
    }

    public async Task<OrderUpdateResult> UpdateAsync(Order order)
    {
        _logger.LogInformation($"Updating order, order items count: {order.OrderItems.Count}");
        try
        {
            var currentOrderInDb = await _orderRepository.GetExistingOrderForUpdate(order.Id);

            // We are doing this because in HandleDeletedOrderItems there is a call of _orderItemRepository.Delete(orderItem);.
            // Later in this method we call await _orderRepository.Update(updatedOrder); ant the error thrown:
            // the entity with the same Id is already tracked. To reslove it we need to reset order properties of each order item.
            foreach (var item in currentOrderInDb.OrderItems)
            {
                item.Order = null;
            }

            _logger.LogTrace($"Upserting sender city {order.SenderNpCity.Present}");
            var senderNpCityId = await _addNpCityService.UpsertNpCityAsync(order.SenderNpCity);

            Guid? recipientNpCityId = null;
            if (order.DeliveryType == DeliveryType.WarehouseOrPost)
            {
                _logger.LogTrace($"Upserting recipient city {order.RecipientNpCity.Present}");
                recipientNpCityId = await _addNpCityService.UpsertNpCityAsync(order.RecipientNpCity);
            }

            Guid senderContactPersonId = currentOrderInDb.SenderContactPersonId;
            if (order.SenderContactPerson.Ref != currentOrderInDb.SenderContactPerson.Ref ||
                order.SenderContactPerson.CounterpartyRef != currentOrderInDb.SenderContactPerson.CounterpartyRef)
            {
                _logger.LogTrace($"Upserting sender contect person {order.SenderContactPerson.Phones}");
                senderContactPersonId = await _addNpContactPersonService.UpsertNpContactPersonAsync(order.SenderContactPerson);
            }

            var existingOrderRecipient = await _orderRecipientRepository.GetOrderRecipientByPhoneNumberAsync(order.OrderRecipient.NpContactPerson.Phones);
            Guid? orderRecipientId = existingOrderRecipient?.Id;
            if (existingOrderRecipient == null)
            {
                _logger.LogTrace($"Adding recipient contact person {order.OrderRecipient.NpContactPerson.MiddleName} (during update phone number has been changed)");

                var addedContactPerson = await _contactPersonService.SaveRecipientCounterparty(
                    order.OrderRecipient.NpContactPerson.FirstName,
                    order.OrderRecipient.NpContactPerson.MiddleName,
                    order.OrderRecipient.NpContactPerson.LastName,
                    order.OrderRecipient.NpContactPerson.Phones);
                OrderRecipient newOrderRecipient = new()
                {
                    InstUrl = order.OrderRecipient.InstUrl,
                    CreatedDate = DateTime.UtcNow,
                    NpContactPerson = new()
                    {
                        LastName = order.OrderRecipient.NpContactPerson.LastName,
                        FirstName = order.OrderRecipient.NpContactPerson.FirstName,
                        MiddleName = order.OrderRecipient.NpContactPerson.MiddleName,
                        Phones = order.OrderRecipient.NpContactPerson.Phones,
                        Ref = addedContactPerson.ContactPerson.Data.First().Ref,
                        CounterpartyRef = addedContactPerson.Ref
                    }
                };

                _logger.LogTrace("Inserting new order recipient and contact person");
                orderRecipientId = await _addOrderRecipientService.UpsertOrderRecipientAsync(newOrderRecipient);
            }
            else
            {
                _logger.LogTrace($"Order recipient with phone number {order.OrderRecipient.NpContactPerson.Phones} exist, updating existing info...");
                await _addOrderRecipientService.UpsertOrderRecipientAsync(order.OrderRecipient);
            }

            List<OrderNpOptionsSeat> npOptionSeats = [];
            _logger.LogTrace("Handling NP options seat update");
            foreach (var orderOptionsSeat in order.OrderNpOptionsSeats)
            {
                var npOptionsSeat = orderOptionsSeat.NpOptionsSeat;
                var oprionsSeatId = await _addNpOptionsSeatService.GetOrCreateNpOptionsSeat(npOptionsSeat);
                npOptionSeats.Add(new() { NpOptionsSeatId = oprionsSeatId });
            }

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
                    OrderRecipientId = orderRecipientId.Value,
                    PayerType = order.PayerType,
                    PaymentMethod = order.PaymentMethod,
                    SaleType = currentOrderInDb.SaleType, // SaleType cannot be updated
                    CreationDateTime = currentOrderInDb.CreationDateTime,
                    SendUntilDate = order.SendUntilDate,
                    Weight = order.Weight,
                    DeliveryType = order.DeliveryType,
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
                };

                var newOrderStatus = order.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
                    ? OrderStatus.InProgress
                    : OrderStatus.ReadyToShip;

                var orderItemsChanged = OrderItemsChanged(currentOrderInDb.OrderItems, order.OrderItems);
                OrderStatusHelper.SetOrderStatus(updatedOrder, newOrderStatus, orderItemsChanged);

                _logger.LogTrace("Updating order in db");
                await _orderRepository.Update(updatedOrder);

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
                _logger.LogError(ex, "An error occured during updating order");
                await transaction.RollbackAsync();
                throw;
            }

            // ---------------------- End Order processing and save ----------------------

            var npInternetDocUpdatedSuccessfully = await UpdateInternetDocument(updatedOrder, currentOrderInDb);

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
                _logger.LogError(ex, "An error occurred during handling prduct variant quantity change");
            }

            return new() { OrderUpdatedSuccessfully = true, NpInternetDocUpdatedSuccessfully = npInternetDocUpdatedSuccessfully };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating order");
            return new();
        }
    }

    private async Task<bool> UpdateInternetDocument(Order updatedOrder, Order currentOrderInDb)
    {
        try
        {
            var orderWithAllInfo = await _orderRepository.GetOrderWithInfoForInternetDocCreation(updatedOrder.Id);
            CreateNovaPostInternetDocument updatedDocument = _internetDocumentService.CreateNovaPostInternetDocument(orderWithAllInfo);
            if (string.IsNullOrEmpty(orderWithAllInfo.InternetDocumentIntDocNumber))
            {
                _logger.LogTrace($"Creating internet document for order {orderWithAllInfo.Id}, because it wasn't previously created");
                var internetDocumentCreationResponse = await _internetDocumentService.CreateInternetDocumentAsync(updatedDocument);

                if (!internetDocumentCreationResponse.Success)
                {
                    var response = JsonConvert.SerializeObject(internetDocumentCreationResponse);
                    _logger.LogError($"An error occurred while creating internet document for order {updatedOrder.Id}: {response}");

                    updatedOrder.InternetDocumentCreationAttempted = true;

                    await _orderRepository.Save();

                    return false;
                }

                _logger.LogInformation($"Internet document for order {updatedOrder.Id} created successfuly");

                updatedOrder.InternetDocumentCreationAttempted = true;
                updatedOrder.InternetDocumentRef = internetDocumentCreationResponse.Data[0].Ref;
                updatedOrder.InternetDocumentIntDocNumber = internetDocumentCreationResponse.Data[0].IntDocNumber;

                await _orderRepository.Save();

                return true;
            }
            else
            {
                if (IntDocUpdateRequired(updatedDocument, currentOrderInDb))
                {
                    _logger.LogTrace($"Updating internet document for order {orderWithAllInfo.Id}");
                    UpdateNovaPostInternetDocument documentToUpdate = _internetDocumentService.CreateUpdateNovaPostInternetDocument(orderWithAllInfo);

                    var internetDocumentCreationResponse = await _internetDocumentService
                        .CreateInternetDocumentAsync(documentToUpdate.CreateNovaPostInternetDocument, documentToUpdate.Ref);

                    if (!internetDocumentCreationResponse.Success)
                    {
                        var response = JsonConvert.SerializeObject(internetDocumentCreationResponse);
                        _logger.LogError($"An error occurred while updating internet document for order {updatedOrder.Id}: {response}");

                        updatedOrder.InternetDocumentCreationAttempted = true;

                        await _orderRepository.Save();

                        return false;
                    }

                    _logger.LogInformation($"Internet document for order {updatedOrder.Id} updated successfuly");

                    updatedOrder.InternetDocumentCreationAttempted = true;
                    updatedOrder.InternetDocumentRef = internetDocumentCreationResponse.Data[0].Ref;
                    updatedOrder.InternetDocumentIntDocNumber = internetDocumentCreationResponse.Data[0].IntDocNumber;

                    await _orderRepository.Save();

                    return true;
                }
                else
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding/updating NP internet document for order");

            updatedOrder.InternetDocumentCreationAttempted = true;

            await _orderRepository.Save();

            return false;
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

        var newProductVariantIds = newOrderItems
            .Select(i => i.ProductVariantId);
        var existingProductVariantIds = existingOrderItemsFromDb
            .Select(i => i.ProductVariantId);
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
        // Funally - new order items, quantities only can be subtracted

        await _orderItemChangeService.HandleDeletedOrderItems(saleType, deletedOrderItems, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder);
        await _orderItemChangeService.HandleUpdatedOrderItems(saleType, existingOrderItems, existingOrderItemsFromDb, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder);

        // ProcessNewOrderItemsStatuses should be called before HandleAddedOrderItems, because in
        // HandleAddedOrderItems product variant quantities are changed and because of that
        // there may be incorrect statuses, if we call ProcessNewOrderItemsStatuses after HandleAddedOrderItems
        _orderItemChangeService.ProcessNewOrderItemsStatuses(addedOrderItems, productVariantsRelatedToCurrentOrder);
        await _orderItemChangeService.HandleAddedOrderItems(saleType, addedOrderItems, productQuantitiesAfterProcessing, productVariantsRelatedToCurrentOrder);
    }

    private bool IntDocUpdateRequired(
        CreateNovaPostInternetDocument updatedDocument,
        Order previousOrder)
    {
        if (InternetDocumentConstants.OrderReceivedStatuses.Contains(previousOrder.OrderStatus))
        {
            return false;
        }

        CreateNovaPostInternetDocument previousDocument = _internetDocumentService.CreateNovaPostInternetDocument(previousOrder);

        var serializationSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        return JsonConvert.SerializeObject(updatedDocument, Formatting.None, serializationSettings) != JsonConvert.SerializeObject(previousDocument, Formatting.None, serializationSettings);
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
