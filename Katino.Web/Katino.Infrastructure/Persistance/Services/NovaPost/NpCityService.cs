using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Katino.Domain.Services.NovaPost.City;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class NpCityService : BaseNpApiService, INpCityService
{
    private readonly HttpClient _httpClient;
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;

    public NpCityService(
        HttpClient httpClient,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory) : base(httpClient, options, loggerFactory)
    {
        _httpClient = httpClient;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(NpCityService));
    }

    public async Task<GetCitiesResponse> GetCities(string cityName, int limit)
    {
        NpApiRequest<object> getCitiesRequest = new()
        {
            ApiKey = _novaPostOptions.ApiKey,
            ModelName = "AddressGeneral",
            CalledMethod = "searchSettlements",
            MethodProperties = new
            {
                CityName = cityName,
                Limit = limit
            }
        };

        var responseString = await GetProcessedStringResponse(getCitiesRequest);
        var getSitiesResponse = JsonConvert.DeserializeObject<NpApiResponse<GetCitiesResponse>>(responseString);

        _logger.LogDebug("GetCities response: {Response}", responseString);

        CheckApiResponse(getSitiesResponse);

        return getSitiesResponse.Data.FirstOrDefault();
    }
}
