using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Exceptions;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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

    public async Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request)
    {
        // InternetDocumentCreationAttempted and other ttn related properties update after ttn creation
        // on UI implement 2, 4, 10 sizes dropdown

        // TODO start with:
        // 9. TTN statuses.
        // 10. On UI on order save city present and DeliveryRef will also be sent in request and will be saved in NpCity table if not exist and the NpCityId will be stored
        //    in Order. On get orders by this NpCityId we can load the requried info. Same should be done in User settings (for cities and warehouses, NpCityId and NpWarehouseID
        //    will be stored in settings and on load all info will be retrieved).

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
            CalledMethod = "save",
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
