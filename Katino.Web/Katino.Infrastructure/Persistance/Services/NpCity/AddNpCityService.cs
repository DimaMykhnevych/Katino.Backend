using Katino.Domain.Entities;
using Katino.Domain.Repositories.NpCityRepository;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.NpCityN;

public class AddNpCityService : IAddNpCityService
{
    private readonly INpCityRepository _npCityRepository;
    private readonly ILogger _logger;

    public AddNpCityService(
        INpCityRepository npCityRepository,
        ILoggerFactory loggerFactory)
    {
        _npCityRepository = npCityRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddNpCityService));
    }

    public async Task<Guid> UpsertNpCityAsync(NpCity npCity)
    {
        var existingNpCity = await _npCityRepository.GetNpCityByDeliveryCityAsync(npCity.DeliveryCity);
        if (existingNpCity == null)
        {
            _logger.LogDebug($"NP city {npCity.Present} ({npCity.DeliveryCity}) doesn't exist in database, adding...");
            var addedCity = await _npCityRepository.Insert(npCity);
            await _npCityRepository.Save();
            return addedCity.Id;
        }

        _logger.LogDebug($"NP city {npCity.Present} ({npCity.DeliveryCity}) exists in database, updating...");

        existingNpCity.Present = npCity.Present;
        await _npCityRepository.Update(existingNpCity);
        await _npCityRepository.Save();
        return existingNpCity.Id;
    }
}
