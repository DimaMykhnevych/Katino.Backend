using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Exceptions;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Repositories.NpWarehouseRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NovaPost.Warehouse;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

// TODO implement caching for getWarehouses, getCounterparties, getCounterpartyContactPersons (after front implementation ???)
// TODO maybe models will be slighlty changed (already Id's of cities will be sent, not names, etc.)
// TODO create endpoint for manual update of warehouses
public class InternetDocumentService : BaseNpApiService, IInternetDocumentService
{
    private const int CacheExpirationHours = 1;

    private readonly HttpClient _httpClient;
    private readonly IWarehouseService _warehouseService;
    private readonly INpWarehouseRepository _npWarehouseRepository;
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;

    public InternetDocumentService(
        HttpClient httpClient,
        IWarehouseService warehouseService,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory,
        IMemoryCache memoryCache,
        INpWarehouseRepository npWarehouseRepository) : base(httpClient, options, loggerFactory)
    {
        _httpClient = httpClient;
        _warehouseService = warehouseService;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(InternetDocumentService));
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(CacheExpirationHours));

        _npWarehouseRepository = npWarehouseRepository;
    }

    public async Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request)
    {
        // Начать с пункта 6, после него разобраться с модельками ордера и сделать сохранение ордера (также сохранять номер ттн), после чего переходить к юайке (также не забыть про страничку с настройками),
        // вначале сделать создание ордера, а потом уже обновление по ходу дела сделать, будет понятнее (флоу апдейта - если новое добавляем, если старое находим в бд и апдейтим)


        //TODO start with:
        // [DONE] 1. [Finish implementing address delivery (remove not required fields)]
        // [DONE] 2. [Manual sync of NP (separate admin controller, update in separate thread)]
        // [DONE] 3. [Store sync status in db]
        // [DONE] 4. [Сделать поиск отделений по номеру добавить соответсвующий ендпоинт]
        // [DONE] 5. [Разобраться с посиком городов, контактных лиц - сделать эндпоинты на гет (возвращать нужные поля только).]
        // [DONE] 6. [Имплементировать настройки юзера срм - город и отделение отправки, там же можно запускать ручное обновление городов (в отдельном потоке).]
        //6.1 Проапдейтить код создания ТТНок, там теперь просто айдишники будут приходить (GetSenderCounterpartyRef придется таки брать с НП АПИ, а вот ContactPerson будет уже приходить извне)
        //7. Потом уже логическое структурирование модели заказа + имплементация сохранения/апдейта/удаление ордера - те, кто что-то заказал - в отдельной таблице (ФИО+номер телефона+инстурла+их recipientCounterparty).
        //8. Имплементировать юайку.
        //9. Разобраться со статусами ттнок.
        //10. On UI on order save city present and DeliveryRef will also be sent in request and will be saved in NpCity table if not exist and the NpCityId will be stored
        //    in Order. On get orders by this NpCityId we can load the requried info. Same should be done in User settings (for cities and warehouses, NpCityId and NpWarehouseID
        //    will be stored in settings and on load all info will be retrieved).
        //    FOR SENDER CONTACT PERSON WE ALSO SHOULD STORE IT IN CONTACT PERSONS

        // TODO remove GetCityRefAsync, GetSenderCounterpartyRef, GetSenderContactPerson, GetWarehousesResponse
        var senderCityRef = await GetCityRefAsync(request.SenderCityName);
        var senderCounterpartyRef = await GetSenderCounterpartyRef();
        var senderContactPerson = await GetSenderContactPerson(senderCounterpartyRef);
        var senderWarehouseResponse = await GetWarehousesResponse(senderCityRef, request.SenderWarehouseId);

        string recipientCityRef = null;
        WarehousesResponse recipientWarehouseResponse = null;
        if (request.DeliveryType != DeliveryType.Address)
        {
            recipientCityRef = await GetCityRefAsync(request.RecipientCityName);
            recipientWarehouseResponse = await GetWarehousesResponse(recipientCityRef, request.RecipientWarehouseId);
        }

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

        SaveCounterpartyGeneralResponse recipientCounterparty = null;
        if (request.DeliveryType != DeliveryType.Address)
        {
            recipientCounterparty = await SaveRecipientCounterparty(
            request.RecipientFirstName,
            request.RecipientMiddleName,
            request.RecipientLastName,
            request.RecipientPhone);
        }

        SaveInternetDocumentRequest saveDocumentRequest;
        if (request.DeliveryType != DeliveryType.Address)
        {
            saveDocumentRequest = new()
            {
                SenderWarehouseIndex = senderWarehouseResponse.WarehouseIndex,
                RecipientWarehouseIndex = recipientWarehouseResponse.WarehouseIndex,
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
                CitySender = senderCityRef,
                Sender = senderCounterpartyRef,
                SenderAddress = senderWarehouseResponse.Ref,
                ContactSender = senderContactPerson.Ref,
                SendersPhone = senderContactPerson.Phones,
                CityRecipient = recipientCityRef,
                Recipient = recipientCounterparty.Ref,
                RecipientAddress = recipientWarehouseResponse.Ref,
                ContactRecipient = recipientCounterparty.ContactPerson.Data.First().Ref,
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
            saveDocumentRequest = 
                GetRequestForAddressDelivery(request, serviceType, senderCityRef, senderCounterpartyRef, senderWarehouseResponse, senderContactPerson);
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

    private SaveInternetDocumentRequest GetRequestForAddressDelivery(
        CreateNovaPostInternetDocument request,
        ServiceType serviceType,
        string senderCityRef,
        string senderCounterpartyRef,
        WarehousesResponse senderWarehouseResponse,
        ContactPersonsResponse senderContactPerson)
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
            CitySender = senderCityRef,
            Sender = senderCounterpartyRef,
            SenderAddress = senderWarehouseResponse.Ref,
            ContactSender = senderContactPerson.Ref,
            SendersPhone = senderContactPerson.Phones,
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

    private async Task<string> GetCityRefAsync(string cityName)
    {
        var cacheKey = $"{nameof(GetCityRefAsync)}_{cityName}";
        if (_memoryCache.TryGetValue(cacheKey, out string cityRef))
        {
            return cityRef;
        }

        NpApiRequest<object> getCityInfoRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "AddressGeneral",
            CalledMethod = "searchSettlements",
            MethodProperties = new
            {
                CityName = cityName,
                Limit = 1
            }
        };

        var json = JsonConvert.SerializeObject(getCityInfoRequest);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_novaPostOptions.BaseUrl, content);

        response.EnsureSuccessStatusCode();

        var cityInfoResponse = await response.Content.ReadFromJsonAsync<NpApiResponse<CityInfoResponse>>();

        _logger.LogDebug("GetCityRef response: {Response}", JsonConvert.SerializeObject(cityInfoResponse));

        CheckApiResponse(cityInfoResponse);

        var result = cityInfoResponse.Data.FirstOrDefault().Addresses.First().DeliveryCity;
        _memoryCache.Set(cacheKey, result, _memoryCacheEntryOptions);

        return result;
    }

    private async Task<string> GetSenderCounterpartyRef()
    {
        if (_memoryCache.TryGetValue(nameof(GetSenderCounterpartyRef), out string senderCounterpartyRef))
        {
            return senderCounterpartyRef;
        }

        NpApiRequest<object> getSenderCounterpartyRefRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "CounterpartyGeneral",
            CalledMethod = "getCounterparties",
            MethodProperties = new
            {
                CounterpartyProperty = "Sender",
                Page = "1"
            }
        };

        var responseString = await GetProcessedStringResponse(getSenderCounterpartyRefRequest);
        var senderResponse = JsonConvert.DeserializeObject<NpApiResponse<SenderCounterpartyResponse>>(responseString);

        _logger.LogDebug("GetSenderCounterpartyRef response: {Response}", responseString);

        CheckApiResponse(senderResponse);

        var result = senderResponse.Data.FirstOrDefault().Ref;
        _memoryCache.Set(nameof(GetSenderCounterpartyRef), result, _memoryCacheEntryOptions);

        return result;
    }

    private async Task<ContactPersonsResponse> GetSenderContactPerson(string senderCounterpartyRef)
    {
        var cacheKey = $"{nameof(GetSenderContactPerson)}_{senderCounterpartyRef}";

        if (_memoryCache.TryGetValue(cacheKey, out ContactPersonsResponse senderContactPersons))
        {
            return senderContactPersons;
        }

        NpApiRequest<object> getSenderContactPersonsRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "CounterpartyGeneral",
            CalledMethod = "getCounterpartyContactPersons",
            MethodProperties = new
            {
                Ref = senderCounterpartyRef,
                Page = "1"
            }
        };

        var responseString = await GetProcessedStringResponse(getSenderContactPersonsRequest);
        var senderContactResponse = JsonConvert.DeserializeObject<NpApiResponse<ContactPersonsResponse>>(responseString);

        _logger.LogDebug("GetSenderContactPerson response: {Response}", responseString);

        CheckApiResponse(senderContactResponse);

        var result = senderContactResponse.Data.FirstOrDefault();

        _memoryCache.Set(cacheKey, result, _memoryCacheEntryOptions);

        return result;
    }

    private async Task<WarehousesResponse> GetWarehousesResponse(string cityRef, string warehouseId)
    {
        // TODO if we are going to pass Id from NpWarehouses table to create TTN request (that id we are going to take from UI imput to search warehouses)
        // this method will be redundant and fallbeck to real API then cannot be implemented.
        // At first manual update will be okay, in future some progress of update can be shown to user and appropriate endpoints should be disabled.
        // To store progress/history of update separate table can be used with updaterequested time, updatefinished time, etc.
        var cacheKey = $"{nameof(GetWarehousesResponse)}_{cityRef}_{warehouseId}";

        if (_memoryCache.TryGetValue(cacheKey, out WarehousesResponse warehouses))
        {
            return warehouses;
        }

        var resultFromDb = await _npWarehouseRepository.GetWarehouseByNumberAndCityRefAsync(warehouseId, cityRef);
        if (resultFromDb != null)
        {
            var mappedResponse = new WarehousesResponse()
            {
                Ref = resultFromDb.Ref,
                CityRef = resultFromDb.CityRef,
                WarehouseIndex = resultFromDb.WarehouseIndex,
                Description = resultFromDb.Description,
                Number = resultFromDb.Number,
                ShortAddress = resultFromDb.ShortAddress,
            };

            _memoryCache.Set(cacheKey, mappedResponse, _memoryCacheEntryOptions);
            return mappedResponse;
        }

        var result = await _warehouseService.SearchWarehousesAsync(cityRef, warehouseId);

        _memoryCache.Set(cacheKey, result, _memoryCacheEntryOptions);

        return result;
    }

    private async Task<SaveCounterpartyGeneralResponse> SaveRecipientCounterparty(
        string firstName,
        string middleName,
        string lastName,
        string phone)
    {
        NpApiRequest<object> saveRecipientCounterpartyRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "CounterpartyGeneral",
            CalledMethod = "save",
            MethodProperties = new
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                Phone = phone,
                Email = string.Empty,
                CounterpartyType = "PrivatePerson",
                CounterpartyProperty = "Recipient",
            }
        };

        var responseString = await GetProcessedStringResponse(saveRecipientCounterpartyRequest);

        var saveRecipientCounterpartyResponse = JsonConvert.DeserializeObject<NpApiResponse<SaveCounterpartyGeneralResponse>>(responseString);

        _logger.LogDebug("SaveRecipientCounterparty response: {Response}", responseString);

        CheckApiResponse(saveRecipientCounterpartyResponse);

        return saveRecipientCounterpartyResponse.Data.FirstOrDefault();
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
