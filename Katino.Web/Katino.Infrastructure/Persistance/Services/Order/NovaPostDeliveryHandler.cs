using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Repositories.FinanceEntryRepository;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Katino.Domain.Services.OrderN.OrderDeliveryHandler;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class NovaPostDeliveryHandler : IOrderDeliveryHandler
{
    private readonly IContactPersonService _contactPersonService;
    private readonly IOrderRecipientRepository _orderRecipientRepository;
    private readonly IAddOrderRecipientService _addOrderRecipientService;
    private readonly IAddNpOptionsSeatService _addNpOptionsSeatService;
    private readonly IInternetDocumentService _internetDocumentService;
    private readonly IOrderRepository _orderRepository;
    private readonly IFinanceEntryRepository _financeEntryRepository;
    private readonly ILogger _logger;

    public NovaPostDeliveryHandler(
        IContactPersonService contactPersonService,
        IOrderRecipientRepository orderRecipientRepository,
        IAddOrderRecipientService addOrderRecipientService,
        IAddNpOptionsSeatService addNpOptionsSeatService,
        IInternetDocumentService internetDocumentService,
        IOrderRepository orderRepository,
        IFinanceEntryRepository financeEntryRepository,
        ILoggerFactory loggerFactory)
    {
        _contactPersonService = contactPersonService;
        _orderRecipientRepository = orderRecipientRepository;
        _addOrderRecipientService = addOrderRecipientService;
        _addNpOptionsSeatService = addNpOptionsSeatService;
        _internetDocumentService = internetDocumentService;
        _orderRepository = orderRepository;
        _financeEntryRepository = financeEntryRepository;
        _logger = loggerFactory?.CreateLogger(nameof(NovaPostDeliveryHandler));
    }

    public async Task<Guid?> ResolveOrderRecipientAsync(Order order)
    {
        _logger.LogTrace($"Getting existing order recipient {order.OrderRecipient.NpContactPerson.Phones}");
        var existingOrderRecipient = await _orderRecipientRepository
            .GetOrderRecipientByPhoneNumberAsync(order.OrderRecipient.NpContactPerson.Phones);

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
            return await _addOrderRecipientService.UpsertOrderRecipientAsync(newOrderRecipient);
        }
        else
        {
            _logger.LogTrace($"Order recipient with phone number {order.OrderRecipient.NpContactPerson.Phones} exists, updating...");
            await _addOrderRecipientService.UpsertOrderRecipientAsync(order.OrderRecipient);
            return existingOrderRecipient.Id;
        }
    }

    public async Task<List<OrderNpOptionsSeat>> ResolveNpOptionSeatsAsync(IEnumerable<OrderNpOptionsSeat> requestedSeats)
    {
        _logger.LogTrace("Handling NP options seat creation");
        List<OrderNpOptionsSeat> npOptionSeats = [];

        foreach (var orderOptionsSeat in requestedSeats)
        {
            var npOptionsSeat = orderOptionsSeat.NpOptionsSeat;
            var optionsSeatId = await _addNpOptionsSeatService.GetOrCreateNpOptionsSeat(npOptionsSeat);
            npOptionSeats.Add(new() { NpOptionsSeatId = optionsSeatId });
        }

        return npOptionSeats;
    }

    public async Task<bool> HandleInternetDocumentOnAddAsync(Order insertedOrder)
    {
        try
        {
            var orderWithAllInfo = await _orderRepository.GetOrderWithInfoForInternetDocCreation(insertedOrder.Id);
            CreateNovaPostInternetDocument document = _internetDocumentService.CreateNovaPostInternetDocument(orderWithAllInfo);

            _logger.LogTrace($"Creating internet document for order {orderWithAllInfo.Id}");
            var response = await _internetDocumentService.CreateInternetDocumentAsync(document);

            if (!response.Success)
            {
                var responseJson = System.Text.Json.JsonSerializer.Serialize(response);
                _logger.LogError($"An error occurred while creating internet document for order {insertedOrder.Id}: {responseJson}");

                insertedOrder.InternetDocumentCreationAttempted = true;
                await _orderRepository.Save();

                return false;
            }

            _logger.LogInformation($"Internet document for order {insertedOrder.Id} created successfully");

            insertedOrder.InternetDocumentCreationAttempted = true;
            insertedOrder.InternetDocumentRef = response.Data[0].Ref;
            insertedOrder.InternetDocumentIntDocNumber = response.Data[0].IntDocNumber;

            var revenue = await _financeEntryRepository.GetOrderRevenueEntryAsync(insertedOrder.Id);
            if (revenue != null)
            {
                revenue.InternetDocumentIntDocNumber = insertedOrder.InternetDocumentIntDocNumber;
                revenue.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _orderRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding NP internet document for order {insertedOrder.Id}");

            insertedOrder.InternetDocumentCreationAttempted = true;
            await _orderRepository.Save();

            return false;
        }
    }

    public async Task<bool> HandleInternetDocumentOnUpdateAsync(Order updatedOrder, Order currentOrderInDb)
    {
        try
        {
            var orderWithAllInfo = await _orderRepository.GetOrderWithInfoForInternetDocCreation(updatedOrder.Id);
            CreateNovaPostInternetDocument updatedDocument = _internetDocumentService.CreateNovaPostInternetDocument(orderWithAllInfo);

            if (string.IsNullOrEmpty(orderWithAllInfo.InternetDocumentIntDocNumber))
            {
                _logger.LogTrace($"Creating internet document for order {orderWithAllInfo.Id}, because it wasn't previously created");
                var creationResponse = await _internetDocumentService.CreateInternetDocumentAsync(updatedDocument);

                if (!creationResponse.Success)
                {
                    var responseJson = JsonConvert.SerializeObject(creationResponse);
                    _logger.LogError($"An error occurred while creating internet document for order {updatedOrder.Id}: {responseJson}");

                    updatedOrder.InternetDocumentCreationAttempted = true;
                    await _orderRepository.Save();

                    return false;
                }

                _logger.LogInformation($"Internet document for order {updatedOrder.Id} created successfully");

                updatedOrder.InternetDocumentCreationAttempted = true;
                updatedOrder.InternetDocumentRef = creationResponse.Data[0].Ref;
                updatedOrder.InternetDocumentIntDocNumber = creationResponse.Data[0].IntDocNumber;

                await _orderRepository.Save();

                return true;
            }
            else
            {
                if (IntDocUpdateRequired(updatedDocument, currentOrderInDb))
                {
                    _logger.LogTrace($"Updating internet document for order {orderWithAllInfo.Id}");
                    UpdateNovaPostInternetDocument documentToUpdate = _internetDocumentService.CreateUpdateNovaPostInternetDocument(orderWithAllInfo);

                    var updateResponse = await _internetDocumentService
                        .CreateInternetDocumentAsync(documentToUpdate.CreateNovaPostInternetDocument, documentToUpdate.Ref);

                    if (!updateResponse.Success)
                    {
                        var responseJson = JsonConvert.SerializeObject(updateResponse);
                        _logger.LogError($"An error occurred while updating internet document for order {updatedOrder.Id}: {responseJson}");

                        updatedOrder.InternetDocumentCreationAttempted = true;
                        await _orderRepository.Save();

                        return false;
                    }

                    _logger.LogInformation($"Internet document for order {updatedOrder.Id} updated successfully");

                    updatedOrder.InternetDocumentCreationAttempted = true;
                    updatedOrder.InternetDocumentRef = updateResponse.Data[0].Ref;
                    updatedOrder.InternetDocumentIntDocNumber = updateResponse.Data[0].IntDocNumber;

                    await _orderRepository.Save();

                    return true;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding/updating NP internet document for order {updatedOrder.Id}");

            updatedOrder.InternetDocumentCreationAttempted = true;
            await _orderRepository.Save();

            return false;
        }
    }

    public async Task<bool> HandleInternetDocumentOnDeleteAsync(Order order)
    {
        try
        {
            if (string.IsNullOrEmpty(order.InternetDocumentRef))
            {
                _logger.LogInformation($"Order {order.Id} doesn't have associated internet document, skipping deletion");
                return true;
            }

            _logger.LogInformation($"Deleting internet document for order {order.Id}");
            return await _internetDocumentService.DeleteInternetDocumentAsync(order.InternetDocumentRef);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting NP internet document for order {order.Id}");
            return false;
        }
    }

    private bool IntDocUpdateRequired(CreateNovaPostInternetDocument updatedDocument, Order previousOrder)
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
        return JsonConvert.SerializeObject(updatedDocument, Formatting.None, serializationSettings)
            != JsonConvert.SerializeObject(previousDocument, Formatting.None, serializationSettings);
    }
}
