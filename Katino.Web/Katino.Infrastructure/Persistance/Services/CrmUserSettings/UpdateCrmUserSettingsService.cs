using Katino.Domain.Entities;
using Katino.Domain.Repositories.CrmUserSettingsRepository;
using Katino.Domain.Services.CrmUserSettingsN.UpdateCrmUserSettingsService;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.CrmUserSettingsN;

public class UpdateCrmUserSettingsService : IUpdateCrmUserSettingsService
{
    private readonly ILogger _logger;
    private readonly ICrmUserSettingsRepository _crmUserSettingsRepository;
    private readonly IAddNpCityService _addNpCityService;

    public UpdateCrmUserSettingsService(
        ICrmUserSettingsRepository crmUserSettingsRepository,
        IAddNpCityService addNpCityService,
        ILoggerFactory loggerFactory)
    {
        _crmUserSettingsRepository = crmUserSettingsRepository;
        _addNpCityService = addNpCityService;
        _logger = loggerFactory?.CreateLogger(nameof(AddCrmUserSettingsService));
    }

    public async Task<bool> UpdateCrmUserSettingsAsync(CrmUserSettings crmUserSettings)
    {
        try
        {
            _logger.LogDebug($"Updating CRM user settings for user {crmUserSettings.AppUserId}");

            var updatedCityId = await _addNpCityService.UpsertNpCityAsync(crmUserSettings.NpCity);
            var entityToUpdate = new CrmUserSettings()
            {
                Id = crmUserSettings.Id,
                AppUserId = crmUserSettings.AppUserId,
                NpCityId = updatedCityId,
                NpWarehouseId = crmUserSettings.NpWarehouseId,
            };

            await _crmUserSettingsRepository.Update(entityToUpdate);
            await _crmUserSettingsRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred during updating CRM user settings for user {crmUserSettings.AppUserId}");
            return false;
        }
    }
}
