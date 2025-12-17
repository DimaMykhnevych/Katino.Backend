using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.UpdateOrderService;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Microsoft.Extensions.Logging;

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
        _logger = loggerFactory?.CreateLogger(nameof(UpdateOrderService));
    }

    public async Task<OrderUpdateResult> UpdateAsync(Order order)
    {
        _logger.LogInformation($"Updating order, order items count: {order.OrderItems.Count}");
        try
        {
            var currentOrderInDb = await _orderRepository.GetExistingOrderForUpdate(order.Id);

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

            _logger.LogTrace("Processing current order items statuses");
            await _orderItemChangeService.ProcessCurrentOrderItemsStatuses(order.OrderItems);

            await using var transaction = await _katinoDbContext.Database.BeginTransactionAsync();

            await HandleOrderItemsUpdate(order.OrderItems, currentOrderInDb.OrderItems, order.SaleType);
            // TODO add order update (with calculated order status) and wrap transaction with try catch
            // as in the order insert

            await transaction.CommitAsync();

            return new();
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
        SaleType saleType)
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

        await HandleDeletedOrderItems(saleType, deletedOrderItems);
        await HandleAddedOrderItems(saleType, addedOrderItems);
        //await HandleUpdatedOrderItems(itemsToUpdate, existingOrderItemsFromDb);
    }

    public async Task HandleAddedOrderItems(SaleType saleType, List<OrderItem> orderItems)
    {
        await _orderItemChangeService.HandleAddedOrderItems(saleType, orderItems);
    }

    private async Task HandleDeletedOrderItems(SaleType saleType, List<OrderItem> deletedItems)
    {
        await _orderItemChangeService.HandleDeletedOrderItems(saleType, deletedItems);
    }
}
