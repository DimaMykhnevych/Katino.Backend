using Katino.Domain.Entities;

namespace Katino.Domain.Services.CrmUserSettingsN.UpdateCrmUserSettingsService;

public interface IUpdateCrmUserSettingsService
{
    Task<bool> UpdateCrmUserSettingsAsync(CrmUserSettings crmUserSettings);
}
