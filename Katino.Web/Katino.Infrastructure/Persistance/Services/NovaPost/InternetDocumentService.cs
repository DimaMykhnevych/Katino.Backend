using Katino.Domain.Entities;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Exceptions;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class InternetDocumentService : BaseNpApiService, IInternetDocumentService
{
    private readonly HttpClient _httpClient;
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;

    public InternetDocumentService(
        HttpClient httpClient,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory) : base(httpClient, options, loggerFactory)
    {
        _httpClient = httpClient;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(InternetDocumentService));
    }

    public async Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request, string existingDocRef = null)
    {
        ServiceType serviceType;
        switch (request.DeliveryType)
        {
            case DeliveryType.WarehouseOrPost:
                serviceType = ServiceType.DoorsWarehouse;
                break;
            case DeliveryType.Address:
                serviceType = ServiceType.WarehouseDoors;
                break;
            default:
                serviceType = ServiceType.DoorsWarehouse;
                break;
        }

        if (request.DeliveryType == DeliveryType.Address)
        {
            ValidateAddressDeliveryFields(request);
        }

        SaveInternetDocumentRequest saveDocumentRequest;
        if (request.DeliveryType != DeliveryType.Address)
        {
            saveDocumentRequest = new()
            {
                Ref = existingDocRef,
                SenderWarehouseIndex = request.SenderWarehouseIndex,
                RecipientWarehouseIndex = request.RecipientWarehouseIndex,
                PayerType = request.PayerType.ToString(),
                PaymentMethod = request.PaymentMethod.ToString(),
                DateTime = DateTime.Now.ToString("dd.MM.yyyy"),
                CargoType = "Cargo",
                Weight = request.Weight.ToString(),
                ServiceType = serviceType.ToString(),
                SeatsAmount = request.SeatsAmount.ToString(),
                Description = request.Description,
                Cost = request.Cost.ToString(),
                AfterpaymentOnGoodsCost = request.AfterpaymentOnGoodsCost?.ToString(),
                CitySender = request.SenderCityRef,
                Sender = request.SenderCounterpartyRef,
                SenderAddress = request.SenderWarehouseRef,
                ContactSender = request.SenderContactPersonRef,
                SendersPhone = request.SenderContactPersonPhones,
                CityRecipient = request.RecipientCityRef,
                Recipient = request.RecipientCounterpartyRef,
                RecipientAddress = request.RecipientWarehouseRef,
                ContactRecipient = request.RecipientContactPersonRef,
                RecipientsPhone = request.RecipientPhone,
                OptionsSeat = request.OptionsSeat.Select(np => new OptionsSeatNpModel()
                {
                    VolumetricWidth = np.VolumetricWidth.ToString(),
                    VolumetricLength = np.VolumetricLength.ToString(),
                    VolumetricHeight = np.VolumetricHeight.ToString(),
                    Weight = np.Weight.ToString(),
                }),
            };
        }
        else
        {
            saveDocumentRequest = GetRequestForAddressDelivery(request, serviceType);
        }

        var npRequest = new NpApiRequest<SaveInternetDocumentRequest>()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "InternetDocumentGeneral",
            CalledMethod = string.IsNullOrEmpty(existingDocRef) ? "save" : "update",
            MethodProperties = saveDocumentRequest
        };

        var serializationSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        var responseString = await GetProcessedStringResponse(npRequest, serializationSettings);
        var saveDocumentResponse = JsonConvert.DeserializeObject<NpApiResponse<NpInternetDocumentCreationResponse>>(responseString);

        _logger.LogDebug("CreateInternetDocumentAsync response: {Response}", responseString);

        CheckApiResponse(saveDocumentResponse);

        return saveDocumentResponse;
    }

    public async Task<bool> DeleteInternetDocumentAsync(string existingDocRef)
    {
        var npRequest = new NpApiRequest<object>()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "InternetDocumentGeneral",
            CalledMethod = "delete",
            MethodProperties = new
            {
                DocumentRefs = existingDocRef
            }
        };

        var responseString = await GetProcessedStringResponse(npRequest);
        var deleteDocumentResponse = JsonConvert.DeserializeObject<NpApiResponse<object>>(responseString);

        _logger.LogDebug("DeleteInternetDocumentAsync response: {Response}", responseString);
        if (deleteDocumentResponse == null || !deleteDocumentResponse.Success)
        {
            _logger.LogError($"An error occurred while deleting internet document: {deleteDocumentResponse.Errors.FirstOrDefault()}");
            return false;
        }

        return true;
    }

    public async Task<JArray> GetIntDocStatuses(List<string> documentNumbers)
    {
        var bodyProperties = documentNumbers.Select(i => new
        {
            DocumentNumber = i,
        });

        var npRequest = new NpApiRequest<object>()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "TrackingDocumentGeneral",
            CalledMethod = "getStatusDocuments",
            MethodProperties = new
            {
                Documents = bodyProperties
            }
        };

        var responseString = await GetProcessedStringResponse(npRequest);
        var getIntDocStatusResponse = JsonConvert.DeserializeObject<NpApiReducedResponse<JObject>>(responseString);

        _logger.LogDebug("GetIntDocStatuses response: {Response}", responseString);
        if (getIntDocStatusResponse == null || !getIntDocStatusResponse.Success)
        {
            _logger.LogError($"An error occurred while getting internet doc statuses: {getIntDocStatusResponse.Errors.FirstOrDefault()}");
            return null;
        }

        return new JArray(getIntDocStatusResponse.Data);
    }

    public CreateNovaPostInternetDocument CreateNovaPostInternetDocument(Order orderWithAllInfo)
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
            OptionsSeat = orderWithAllInfo.OrderNpOptionsSeats.Select(s => new NpOptionsSeat()
            {
                VolumetricWidth = s.NpOptionsSeat.VolumetricWidth,
                VolumetricLength = s.NpOptionsSeat.VolumetricLength,
                VolumetricHeight = s.NpOptionsSeat.VolumetricHeight,
                Weight = s.NpOptionsSeat.Weight,
            }),

            RecipientAddressNote = orderWithAllInfo.AddressInfo?.RecipientAddressNote,
            RecipientCityName = orderWithAllInfo.AddressInfo?.RecipientCity,
            RecipientAddressName = orderWithAllInfo.AddressInfo?.RecipientAddressName,
            RecipientHouse = orderWithAllInfo.AddressInfo?.RecipientHouse,
            RecipientFlat = orderWithAllInfo.AddressInfo?.RecipientFlat,
        };

        return createIntDocRequest;
    }

    public UpdateNovaPostInternetDocument CreateUpdateNovaPostInternetDocument(Order orderWithAllInfo)
    {
        UpdateNovaPostInternetDocument updateIntDocRequest = new()
        {
            Ref = orderWithAllInfo.InternetDocumentRef,
            CreateNovaPostInternetDocument = CreateNovaPostInternetDocument(orderWithAllInfo)
        };

        return updateIntDocRequest;
    }

    private SaveInternetDocumentRequest GetRequestForAddressDelivery(CreateNovaPostInternetDocument request, ServiceType serviceType)
    {
        return new()
        {
            PayerType = request.PayerType.ToString(),
            PaymentMethod = request.PaymentMethod.ToString(),
            DateTime = DateTime.Now.ToString("dd.MM.yyyy"),
            CargoType = "Cargo",
            Weight = request.Weight.ToString(),
            ServiceType = serviceType.ToString(),
            SeatsAmount = request.SeatsAmount.ToString(),
            Description = request.Description,
            Cost = request.Cost.ToString(),
            AfterpaymentOnGoodsCost = request.AfterpaymentOnGoodsCost?.ToString(),
            CitySender = request.SenderCityRef,
            Sender = request.SenderCounterpartyRef,
            SenderAddress = request.SenderWarehouseRef,
            ContactSender = request.SenderContactPersonRef,
            SendersPhone = request.SenderContactPersonPhones,
            RecipientsPhone = request.RecipientPhone,
            OptionsSeat = request.OptionsSeat.Select(np => new OptionsSeatNpModel()
            {
                VolumetricWidth = np.VolumetricWidth.ToString(),
                VolumetricLength = np.VolumetricLength.ToString(),
                VolumetricHeight = np.VolumetricHeight.ToString(),
                Weight = np.Weight.ToString(),
            }),

            // Address delivery properties
            RecipientAddressNote = request.RecipientAddressNote,
            NewAddress = "1",
            RecipientCityName = request.RecipientCityName,
            RecipientArea = string.Empty,
            RecipientAreaRegions = string.Empty,
            RecipientAddressName = request.RecipientAddressName,
            RecipientHouse = request.RecipientHouse,
            RecipientFlat = request.RecipientFlat,
            RecipientName = $"{request.RecipientLastName} {request.RecipientFirstName} {request.RecipientMiddleName}",
            RecipientType = "PrivatePerson",
            SettlementType = string.Empty,
            RecipientContactName = $"{request.RecipientLastName} {request.RecipientFirstName} {request.RecipientMiddleName}",
            EDRPOU = string.Empty,
        };
    }

    private void ValidateAddressDeliveryFields(CreateNovaPostInternetDocument request)
    {
        List<string> requiredFields = new()
        {
            request.RecipientAddressNote, request.RecipientCityName, request.RecipientAddressName,
            request.RecipientHouse, request.RecipientFlat, request.RecipientMiddleName,
        };

        foreach (var field in requiredFields)
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new NpInternalException("RecipientAddressNote, RecipientCityName, RecipientAddressName" +
                    ", RecipientHouse, RecipientFlat,RecipientName, RecipientMiddleName are required for DeliveryType = Address");
            }
        }
    }
}
