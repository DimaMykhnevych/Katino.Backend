using Katino.Domain.Entities;
using Katino.Domain.Repositories.CrmUserSettingsRepository;
using Katino.Domain.Services.CrmUserSettingsN.AddCrmUserSettingsService;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.CrmUserSettingsN;

public class AddCrmUserSettingsService : IAddCrmUserSettingsService
{
    private readonly ILogger _logger;
    private readonly ICrmUserSettingsRepository _crmUserSettingsRepository;
    private readonly IAddNpCityService _addNpCityService;

    public AddCrmUserSettingsService(
        ICrmUserSettingsRepository crmUserSettingsRepository,
        IAddNpCityService addNpCityService,
        ILoggerFactory loggerFactory)
    {
        _crmUserSettingsRepository = crmUserSettingsRepository;
        _addNpCityService = addNpCityService;
        _logger = loggerFactory?.CreateLogger(nameof(AddCrmUserSettingsService));
    }

    public async Task<bool> AddCrmUserSettingsAsync(CrmUserSettings crmUserSettings)
    {
        try
        {
            _logger.LogDebug($"Adding CRM user settings for user {crmUserSettings.AppUserId}");

            var addedCityId = await _addNpCityService.UpsertNpCityAsync(crmUserSettings.NpCity);
            var entityToAdd = new CrmUserSettings()
            {
                AppUserId = crmUserSettings.AppUserId,
                NpCityId = addedCityId,
                NpWarehouseId = crmUserSettings.NpWarehouseId,
            };

            await _crmUserSettingsRepository.Insert(entityToAdd);
            await _crmUserSettingsRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during adding CRM user settings for user {crmUserSettings.AppUserId}");
            return false;
        }
    }
}
