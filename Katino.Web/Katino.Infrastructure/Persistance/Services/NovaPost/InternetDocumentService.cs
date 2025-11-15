using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Exceptions;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

// TODO implement caching for getWarehouses, getCounterparties, getCounterpartyContactPersons (after front implementation ???)
// TODO maybe models will be slighlty changed (already Id's of cities will be sent, not names, etc.)
public class InternetDocumentService : IInternetDocumentService
{
    private const int CacheExpirationHours = 1;

    private readonly HttpClient _httpClient;
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _memoryCacheEntryOptions;

    public InternetDocumentService(
        HttpClient httpClient,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory,
        IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(InternetDocumentService));
        _memoryCache = memoryCache;
        _memoryCacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(CacheExpirationHours));
    }

    public async Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request)
    {
        var senderCityRef = await GetCityRefAsync(request.SenderCityName);
        var senderCounterpartyRef = await GetSenderCounterpartyRef();
        var senderContactPerson = await GetSenderContactPerson(senderCounterpartyRef);
        var senderWarehouseResponse = await GetWarehousesResponse(senderCityRef, request.SenderWarehouseId);

        var recipientCityRef = await GetCityRefAsync(request.RecipientCityName);
        var recipientWarehouseResponse = await GetWarehousesResponse(recipientCityRef, request.RecipientWarehouseId);

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

        var recipientCounterparty = await SaveRecipientCounterparty(
            request.RecipientFirstName,
            request.RecipientMiddleName,
            request.RecipientLastName,
            request.RecipientPhone);

        SaveInternetDocumentRequest saveDocumentRequest = new()
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
        var cacheKey = $"{nameof(GetWarehousesResponse)}_{cityRef}_{warehouseId}";

        if (_memoryCache.TryGetValue(cacheKey, out WarehousesResponse warehouses))
        {
            return warehouses;
        }

        NpApiRequest<object> getSenderWarehouseRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "AddressGeneral",
            CalledMethod = "getWarehouses",
            MethodProperties = new
            {
                CityRef = cityRef,
                WarehouseId = warehouseId
            }
        };

        var responseString = await GetProcessedStringResponse(getSenderWarehouseRequest);
        var getWarehouseResponse = JsonConvert.DeserializeObject<NpApiResponse<WarehousesResponse>>(responseString);

        _logger.LogDebug("GetWarehousesResponse response: {Response}", responseString);

        CheckApiResponse(getWarehouseResponse);

        var result = getWarehouseResponse.Data.FirstOrDefault();
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

    private async Task<string> GetProcessedStringResponse<T>(NpApiRequest<T> request, JsonSerializerSettings jsonSerializerSettings = null)
    {
        var json = string.Empty;
        if (jsonSerializerSettings == null)
        {
            json = JsonConvert.SerializeObject(request);
        }
        else
        {
            json = JsonConvert.SerializeObject(request, Formatting.Indented, jsonSerializerSettings);
        }

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_novaPostOptions.BaseUrl, content);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        return DecodeUnicodeOnly(responseString);
    }

    private string DecodeUnicodeOnly(string value)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            value,
            @"\\u([0-9a-fA-F]{4})",
            match => ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString()
        );
    }

    private void CheckApiResponse<T>(NpApiResponse<T> response)
    {
        if (response == null || !response.Success)
        {
            _logger.LogError($"An error occurred while communicating with NP API: {response.Errors.FirstOrDefault()}");
            throw new NpInternalException();
        }
    }
}
