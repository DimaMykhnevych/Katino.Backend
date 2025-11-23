using Katino.Domain.Models.NovaPost;

namespace Katino.Domain.Services.NovaPost.City;

public interface INpCityService
{
    Task<GetCitiesResponse> GetCities(string cityName, int limit);
}

