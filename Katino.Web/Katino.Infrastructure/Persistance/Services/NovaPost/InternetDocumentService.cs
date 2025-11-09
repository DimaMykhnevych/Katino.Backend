using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class InternetDocumentService : IInternetDocumentService
{
    private readonly HttpClient _httpClient;
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;

    public InternetDocumentService(
        HttpClient httpClient,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(InternetDocumentService));
    }

    public async Task<NpApiResponse<NpInternetDocumentCreationResponse>> CreateInternetDocumentAsync(CreateNovaPostInternetDocument request)
    {
        var senderCityRef = await GetCityRefAsync(request.SenderCityName);
        var senderCounterpartyRef = await GetSenderCounterpartyRef();
        var senderContactPerson = await GetSenderContactPerson(senderCounterpartyRef);
        var senderWarehouseResponse = await GetWarehousesResponse(senderCityRef, request.SenderWarehouseId);

        var recipientCityRef = await GetCityRefAsync(request.RecipientCityName);
        var recipientWarehouseResponse = await GetWarehousesResponse(recipientCityRef, request.RecipientWarehouseId);

        return new();
    }

    private async Task<string> GetCityRefAsync(string cityName)
    {
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
        return cityInfoResponse.Data.FirstOrDefault().Addresses.First().DeliveryCity;
    }

    private async Task<string> GetSenderCounterpartyRef()
    {
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

        var json = JsonConvert.SerializeObject(getSenderCounterpartyRefRequest);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_novaPostOptions.BaseUrl, content);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        responseString = System.Text.RegularExpressions.Regex.Unescape(responseString);
        var senderResponse = JsonConvert.DeserializeObject<NpApiResponse<SenderCounterpartyResponse>>(responseString);

        return senderResponse.Data.FirstOrDefault().Ref;
    }

    private async Task<ContactPersonsResponse> GetSenderContactPerson(string senderCounterpartyRef)
    {
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

        var json = JsonConvert.SerializeObject(getSenderContactPersonsRequest);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_novaPostOptions.BaseUrl, content);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        responseString = System.Text.RegularExpressions.Regex.Unescape(responseString);
        var senderContactResponse = JsonConvert.DeserializeObject<NpApiResponse<ContactPersonsResponse>>(responseString);

        return senderContactResponse.Data.FirstOrDefault();
    }

    private async Task<WarehousesResponse> GetWarehousesResponse(string cityRef, string warehouseId)
    {
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

        var json = JsonConvert.SerializeObject(getSenderWarehouseRequest);

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_novaPostOptions.BaseUrl, content);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();

        responseString = System.Text.RegularExpressions.Regex.Unescape(responseString);
        var senderContactResponse = JsonConvert.DeserializeObject<NpApiResponse<WarehousesResponse>>(responseString);

        return senderContactResponse.Data.FirstOrDefault();
    }
}
