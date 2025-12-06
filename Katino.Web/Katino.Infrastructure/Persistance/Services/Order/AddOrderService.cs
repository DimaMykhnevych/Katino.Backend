using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Models;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Katino.Domain.Services.OrderN.AddOrderService;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

// TODO cover with logs
public class AddOrderService : IAddOrderService
{
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IAddNpCityService _addNpCityService;
    private readonly IAddNpContactPersonService _addNpContactPersonService;
    private readonly IAddOrderRecipientService _addOrderRecipientService;
    private readonly IContactPersonService _contactPersonService;
    private readonly IOrderRecipientRepository _orderRecipientRepository;
    private readonly IAddNpOptionsSeatService _addNpOptionsSeatService;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public AddOrderService(
        IInternetDocumentService internetDocumentService,
        IAddNpCityService addNpCityService,
        IAddNpContactPersonService addNpContactPersonService,
        IAddOrderRecipientService addOrderRecipientService,
        IContactPersonService contactPersonService,
        IOrderRecipientRepository orderRecipientRepository,
        IAddNpOptionsSeatService addNpOptionsSeatService,
        IProductVariantRepository productVariantRepository,
        IOrderRepository orderRepository,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _addNpCityService = addNpCityService;
        _addNpContactPersonService = addNpContactPersonService;
        _addOrderRecipientService = addOrderRecipientService;
        _contactPersonService = contactPersonService;
        _orderRecipientRepository = orderRecipientRepository;
        _addNpOptionsSeatService = addNpOptionsSeatService;
        _productVariantRepository = productVariantRepository;
        _orderRepository = orderRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddOrderService));
    }

    public async Task<OrderCreationResult> AddAsync(Order order)
    {
        try
        {
            // Handling adding order
            var senderNpCityId = await _addNpCityService.UpsertNpCityAsync(order.SenderNpCity);

            Guid? recipientNpCityId = null;
            if (order.DeliveryType == DeliveryType.WarehouseOrPost)
            {
                recipientNpCityId = await _addNpCityService.UpsertNpCityAsync(order.RecipientNpCity);
            }

            var senderContactPersonId = await _addNpContactPersonService.UpsertNpContactPersonAsync(order.SenderContactPerson);

            var existingOrderRecipient = await _orderRecipientRepository.GetOrderRecipientByPhoneNumberAsync(order.OrderRecipient.NpContactPerson.Phones);
            Guid? orderRecipientId = existingOrderRecipient?.Id;
            if (existingOrderRecipient == null)
            {
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
                        Ref = addedContactPerson.Ref,
                        CounterpartyRef = addedContactPerson.ContactPerson.Data.First().Ref
                    }
                };

                orderRecipientId = await _addOrderRecipientService.UpsertOrderRecipientAsync(newOrderRecipient);
            }

            List<OrderNpOptionsSeat> npOptionSeats = [];
            foreach (var orderOptionsSeat in order.OrderNpOptionsSeats)
            {
                var npOptionsSeat = orderOptionsSeat.NpOptionsSeat;
                var oprionsSeatId = await _addNpOptionsSeatService.GetOrCreateNpOptionsSeat(npOptionsSeat);
                npOptionSeats.Add(new() { NpOptionsSeatId = oprionsSeatId });
            }

            // 1. Process orderItems (set quantity to produce + order item statuses)
            await ProcessNewOrderItems(order.OrderItems);

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
            var insertedOrder = await _orderRepository.Insert(orderToAdd);
            await _orderRepository.Save();

            // 4. Update QuantityInStock (+ ProductVariantStatus InStock or OnOrder) <- only for product variants in order items with status ProductStatus.InStock.
            // QuantityRegularSold QuantityDropSold <-- for all product variant items
            await UpdateProductVariantsQuantities(order.SaleType, order.OrderItems);

            // 7. On Product variant update (quantity in stock) go through all orders that have such order item and update order status and order items, etc.
            // 7.1 Do following actions when sewer completes their work (update order status and order items).
            // 7.2 On order delete go through all orders that have such order item and update order status and order items
            //     On delete recalculate QuantityInStock for product variants and then analyze existing orders, maybe some orders can be fulfilled, if yes - then reduce product variant amount
            //     Do the same action for order update (order items may be added, removed, quantity changed)
            // 8. Cover with logs.
            // 9. Test order creation (add dtos firstly)

            try
            {
                // 5. Save ttn (creating internet document)
                CreateNovaPostInternetDocument document = await CreateNovaPostInternetDocument(insertedOrder.Id);

                var internetDocumentCreationResponse = await _internetDocumentService.CreateInternetDocumentAsync(document);
                if (!internetDocumentCreationResponse.Success)
                {
                    var response = JsonSerializer.Serialize(internetDocumentCreationResponse);
                    _logger.LogError($"An error occurred while creating internet document for order {order.Id}: {response}");

                    // 6. TODO Update ttn related order properties
                    // TODO after NP document creation populate InternetDocumentCreationAttempted, InternetDocumentRef, InternetDocumentIntDocNumber
                    return new() { OrderAddedSuccessfully = true };
                }

                _logger.LogInformation($"Internet document for order {order.Id} created successfuly");

                // 6. TODO Update ttn related order properties
                // TODO after NP document creation populate InternetDocumentCreationAttempted, InternetDocumentRef, InternetDocumentIntDocNumber

                return new() { OrderAddedSuccessfully = true, NpInternetDocCreatedSuccessfully = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while adding NP internet document for order");
                return new() { OrderAddedSuccessfully = true };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding order");
            return new();
        }
    }

    public async Task UpdateProductVariantsQuantities(SaleType saleType, List<OrderItem> orderItems)
    {
        // Updating QuantityInStock, QuantityRegularSold and QuantityDropSold + ProductVariant status
        foreach (var orderItem in orderItems)
        {
            var productVariant = await _productVariantRepository.Get(orderItem.ProductVariantId);
            if (saleType == SaleType.Retail)
            {
                productVariant.QuantityRegularSold += 1;
            }
            else if (saleType == SaleType.Drop || saleType == SaleType.Wholesale)
            {
                productVariant.QuantityDropSold += 1;
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
    }

    private async Task ProcessNewOrderItems(List<OrderItem> orderItems)
    {
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
    }

    private async Task<CreateNovaPostInternetDocument> CreateNovaPostInternetDocument(Guid orderId)
    {
        var orderWithAllInfo = await _orderRepository.GetOrderWithInfoForInternetDocCreation(orderId);

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
