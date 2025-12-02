using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Models;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
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
    private readonly ILogger _logger;

    public AddOrderService(
        IInternetDocumentService internetDocumentService,
        IAddNpCityService addNpCityService,
        IAddNpContactPersonService addNpContactPersonService,
        IAddOrderRecipientService addOrderRecipientService,
        IContactPersonService contactPersonService,
        IOrderRecipientRepository orderRecipientRepository,
        ILoggerFactory loggerFactory)
    {
        _internetDocumentService = internetDocumentService;
        _addNpCityService = addNpCityService;
        _addNpContactPersonService = addNpContactPersonService;
        _addOrderRecipientService = addOrderRecipientService;
        _contactPersonService = contactPersonService;
        _orderRecipientRepository = orderRecipientRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddOrderService));
    }

    public async Task<OrderCreationResult> AddAsync(Order order, CreateNovaPostInternetDocument document)
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

                // TODO implement OrderNpOptionsSeats, AddressInfo
            };

            // TODO after NP document creation populate InternetDocumentCreationAttempted, InternetDocumentRef, InternetDocumentIntDocNumber

            // Handling creating internet document
            try
            {
                // !!!!!!!TODO firstly save this info in database - probably document and not the model of NP request!!!!!!!!!!

                var internetDocumentCreationResponse = await _internetDocumentService.CreateInternetDocumentAsync(document);
                if (!internetDocumentCreationResponse.Success)
                {
                    var response = JsonSerializer.Serialize(internetDocumentCreationResponse);
                    _logger.LogError($"An error occurred while creating internet document for order {order.Id}: {response}");
                    return new() { OrderAddedSuccessfully = true };
                }

                _logger.LogInformation($"Internet document for order {order.Id} created successfuly");

                // After successful creation update order with required info

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
}
