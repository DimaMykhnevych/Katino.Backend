using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Models;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.AddOrderService;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class AddOrderService : IAddOrderService
{
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IAddNpCityService _addNpCityService;
    private readonly IAddNpContactPersonService _addNpContactPersonService;
    private readonly IAddOrderRecipientService _addOrderRecipientService;
    private readonly IContactPersonService _contactPersonService;
    private readonly IOrderRecipientRepository _orderRecipientRepository;
    private readonly IAddNpOptionsSeatService _addNpOptionsSeatService;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IOrderRepository _orderRepository;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;

    public AddOrderService(
        IInternetDocumentService internetDocumentService,
        IAddNpCityService addNpCityService,
        IAddNpContactPersonService addNpContactPersonService,
        IAddOrderRecipientService addOrderRecipientService,
        IContactPersonService contactPersonService,
        IOrderRecipientRepository orderRecipientRepository,
        IAddNpOptionsSeatService addNpOptionsSeatService,
        IOrderItemChangeService orderItemChangeService,
        IOrderRepository orderRepository,
        IKatinoDbContext katinoDbContext,
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
        _logger = loggerFactory?.CreateLogger(nameof(AddOrderService));
    }

    public async Task<OrderCreationResult> AddAsync(Order order)
    {
        _logger.LogInformation($"Adding order, order items count: {order.OrderItems.Count}");

        try
        {
            // Handling adding order
            _logger.LogTrace($"Upserting sender city {order.SenderNpCity.Present}");
            var senderNpCityId = await _addNpCityService.UpsertNpCityAsync(order.SenderNpCity);

            Guid? recipientNpCityId = null;
            if (order.DeliveryType == DeliveryType.WarehouseOrPost)
            {
                _logger.LogTrace($"Upserting recipient city {order.RecipientNpCity.Present}");
                recipientNpCityId = await _addNpCityService.UpsertNpCityAsync(order.RecipientNpCity);
            }

            _logger.LogTrace($"Upserting sender contect person {order.SenderContactPerson.Phones}");
            var senderContactPersonId = await _addNpContactPersonService.UpsertNpContactPersonAsync(order.SenderContactPerson);

            _logger.LogTrace($"Getting existing order recipient {order.OrderRecipient.NpContactPerson.Phones}");
            var existingOrderRecipient = await _orderRecipientRepository.GetOrderRecipientByPhoneNumberAsync(order.OrderRecipient.NpContactPerson.Phones);
            Guid? orderRecipientId = existingOrderRecipient?.Id;
            if (existingOrderRecipient == null)
            {
                _logger.LogTrace($"Adding recipient contact person {order.OrderRecipient.NpContactPerson.MiddleName}");

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

                _logger.LogTrace("Inserting order recipient and contact person");
                orderRecipientId = await _addOrderRecipientService.UpsertOrderRecipientAsync(newOrderRecipient);
            }
            else
            {
                _logger.LogTrace($"Order recipient with phone number {order.OrderRecipient.NpContactPerson.Phones} exist, updating existing info...");
                await _addOrderRecipientService.UpsertOrderRecipientAsync(order.OrderRecipient);
            }

            List<OrderNpOptionsSeat> npOptionSeats = [];

            _logger.LogTrace("Handling NP options seat creation");
            foreach (var orderOptionsSeat in order.OrderNpOptionsSeats)
            {
                var npOptionsSeat = orderOptionsSeat.NpOptionsSeat;
                var oprionsSeatId = await _addNpOptionsSeatService.GetOrCreateNpOptionsSeat(npOptionsSeat);
                npOptionSeats.Add(new() { NpOptionsSeatId = oprionsSeatId });
            }

            // 1. Process orderItems (set quantity to produce + order item statuses)
            _logger.LogTrace("Processing order items statuses");
            await _orderItemChangeService.ProcessCurrentOrderItemsStatuses(order.OrderItems);

            Order orderToAdd = new()
            {
                SenderNpWarehouseId = order.SenderNpWarehouseId,
                RecipientNpWarehouseId = order.RecipientNpWarehouseId,
                SenderNpCityId = senderNpCityId,
                RecipientNpCityId = recipientNpCityId,
                SenderContactPersonId = senderContactPersonId,
                OrderRecipientId = orderRecipientId.Value,
                PayerType = order.PayerType,
                PaymentMethod = order.PaymentMethod,
                SaleType = order.SaleType,
                CreationDateTime = DateTime.UtcNow,
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
            };

            // 2.Calculate Order status and add it to order
            orderToAdd.OrderReadinessStatus = order.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
                ? OrderReadinessStatus.InProgress
                : OrderReadinessStatus.ReadyToShip;

            // 3. Save order
            _logger.LogTrace("Saving order in db");
            await using var transaction = await _katinoDbContext.Database.BeginTransactionAsync();

            var insertedOrder = await _orderRepository.Insert(orderToAdd);
            try
            {
                await _orderRepository.Save();

                // 4. Update QuantityInStock (+ ProductVariantStatus InStock or OnOrder) <- only for product variants in order items with status ProductStatus.InStock.
                // QuantityRegularSold QuantityDropSold <-- for all product variant items
                _logger.LogTrace("Updting order product variant quentities (handling added order items)");
                await HandleAddedOrderItems(order.SaleType, order.OrderItems);
                await transaction.CommitAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occured during saving order and updating product variant quantities");
                await transaction.RollbackAsync();

                throw;
            }

            // TODO
            // 7.1 Implement order update (with ttn update if it was successfully created), delete (with ttn deletion), get (on get - firstly get actual NP statuses, save in db and then show, also on UI separate control to get actual statuses(for completed NP statuses not calling API again), maybe pagination should be used)
            // 7.2 Do following actions when sewer completes their work (update order status and order items, set order item completed date).
            // 7.3 On order delete go through all orders that have such order item and update order status and order items
            //     On delete recalculate QuantityInStock for product variants and then analyze existing orders, maybe some orders can be fulfilled, if yes - then reduce product variant amount
            //     Do the same action for order update (order items may be added, removed, quantity changed)

            try
            {
                // 5. Save ttn (creating internet document)
                var orderWithAllInfo = await _orderRepository.GetOrderWithInfoForInternetDocCreation(insertedOrder.Id);
                CreateNovaPostInternetDocument document = CreateNovaPostInternetDocument(orderWithAllInfo);

                _logger.LogTrace($"Creating internet document for order {orderWithAllInfo.Id}");
                var internetDocumentCreationResponse = await _internetDocumentService.CreateInternetDocumentAsync(document);
                if (!internetDocumentCreationResponse.Success)
                {
                    var response = JsonSerializer.Serialize(internetDocumentCreationResponse);
                    _logger.LogError($"An error occurred while creating internet document for order {order.Id}: {response}");

                    insertedOrder.InternetDocumentCreationAttempted = true;

                    // We don't use _orderRepository.Update, because this instance already tracked by _orderRepository.Insert
                    await _orderRepository.Save();

                    return new() { OrderAddedSuccessfully = true };
                }

                _logger.LogInformation($"Internet document for order {order.Id} created successfuly");

                insertedOrder.InternetDocumentCreationAttempted = true;
                insertedOrder.InternetDocumentRef = internetDocumentCreationResponse.Data[0].Ref;
                insertedOrder.InternetDocumentIntDocNumber = internetDocumentCreationResponse.Data[0].IntDocNumber;

                // We don't use _orderRepository.Update, because this instance already tracked by _orderRepository.Insert
                await _orderRepository.Save();

                return new() { OrderAddedSuccessfully = true, NpInternetDocCreatedSuccessfully = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding NP internet document for order");

                insertedOrder.InternetDocumentCreationAttempted = true;

                // We don't use _orderRepository.Update, because this instance already tracked by _orderRepository.Insert
                await _orderRepository.Save();

                return new() { OrderAddedSuccessfully = true };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding order");
            return new();
        }
    }

    public async Task HandleAddedOrderItems(SaleType saleType, List<OrderItem> orderItems)
    {
        await _orderItemChangeService.HandleAddedOrderItems(saleType, orderItems);
    }

    private CreateNovaPostInternetDocument CreateNovaPostInternetDocument(Order orderWithAllInfo)
    {
        CreateNovaPostInternetDocument createIntDocRequest = new() 
        {
            SenderCityRef = orderWithAllInfo.SenderNpCity.DeliveryCity,
            SenderCounterpartyRef = orderWithAllInfo.SenderContactPerson.CounterpartyRef,
            SenderContactPersonRef = orderWithAllInfo.SenderContactPerson.Ref,
            SenderContactPersonPhones = orderWithAllInfo.SenderContactPerson.Phones,
            SenderWarehouseIndex = orderWithAllInfo.SenderNpWarehouse.WarehouseIndex,
            SenderWarehouseRef = orderWithAllInfo.SenderNpWarehouse.Ref,

            RecipientCityRef = orderWithAllInfo.RecipientNpCity?.DeliveryCity,
            RecipientCounterpartyRef = orderWithAllInfo.OrderRecipient.NpContactPerson.CounterpartyRef,
            RecipientContactPersonRef = orderWithAllInfo.OrderRecipient.NpContactPerson.Ref,
            RecipientPhone = orderWithAllInfo.OrderRecipient.NpContactPerson.Phones,
            RecipientWarehouseIndex = orderWithAllInfo.RecipientNpWarehouse?.WarehouseIndex,
            RecipientWarehouseRef = orderWithAllInfo.RecipientNpWarehouse?.Ref,
            RecipientFirstName = orderWithAllInfo.OrderRecipient.NpContactPerson.FirstName,
            RecipientMiddleName = orderWithAllInfo.OrderRecipient.NpContactPerson.MiddleName,
            RecipientLastName = orderWithAllInfo.OrderRecipient.NpContactPerson.LastName,

            DeliveryType = orderWithAllInfo.DeliveryType,
            PayerType = orderWithAllInfo.PayerType,
            PaymentMethod = orderWithAllInfo.PaymentMethod,
            Weight = orderWithAllInfo.Weight,
            SeatsAmount = orderWithAllInfo.SeatsAmount,
            Description = orderWithAllInfo.Description,
            Cost = orderWithAllInfo.Cost,
            AfterpaymentOnGoodsCost = orderWithAllInfo.AfterpaymentOnGoodsCost,
            OptionsSeat = orderWithAllInfo.OrderNpOptionsSeats.Select(s => s.NpOptionsSeat),

            RecipientAddressNote = orderWithAllInfo.AddressInfo?.RecipientAddressNote,
            RecipientCityName = orderWithAllInfo.AddressInfo?.RecipientCity,
            RecipientAddressName = orderWithAllInfo.AddressInfo?.RecipientAddressName,
            RecipientHouse = orderWithAllInfo.AddressInfo?.RecipientHouse,
            RecipientFlat = orderWithAllInfo.AddressInfo?.RecipientFlat,
        };

        return createIntDocRequest;
    }
}
