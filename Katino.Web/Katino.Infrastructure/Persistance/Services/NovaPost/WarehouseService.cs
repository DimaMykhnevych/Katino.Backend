using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Services.NovaPost.Warehouse;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class WarehouseService : BaseNpApiService, IWarehouseService
{
    private readonly HttpClient _httpClient;
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;

    public WarehouseService(
        HttpClient httpClient,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory) : base(httpClient, options, loggerFactory)
    {
        _httpClient = httpClient;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(WarehouseService));
    }


    public async Task<IEnumerable<WarehousesResponse>> GetWarehousesWithPaginationAsync(string page, string limit)
    {
        NpApiRequest<object> getWarehouseRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "AddressGeneral",
            CalledMethod = "getWarehouses",
            MethodProperties = new
            {
                Page = page,
                Limit = limit
            }
        };

        var responseString = await GetProcessedStringResponse(getWarehouseRequest);
        var getWarehouseResponse = JsonConvert.DeserializeObject<NpApiResponse<WarehousesResponse>>(responseString);

        if (getWarehouseResponse == null || !getWarehouseResponse.Success)
        {
            _logger.LogError($"An error occurred while communicating with NP API: {getWarehouseResponse.Errors.FirstOrDefault()}");
            return [];
        }

        return getWarehouseResponse.Data;
    }

    public async Task<WarehousesResponse> SearchWarehousesAsync(string cityRef, string warehouseId)
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

        var responseString = await GetProcessedStringResponse(getSenderWarehouseRequest);
        var getWarehouseResponse = JsonConvert.DeserializeObject<NpApiResponse<WarehousesResponse>>(responseString);

        _logger.LogDebug("GetWarehousesResponse response: {Response}", responseString);

        CheckApiResponse(getWarehouseResponse);

        return getWarehouseResponse.Data.FirstOrDefault();
    }
}
