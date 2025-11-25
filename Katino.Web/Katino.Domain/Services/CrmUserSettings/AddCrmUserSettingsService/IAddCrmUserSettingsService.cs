using Katino.Domain.Entities;

namespace Katino.Domain.Services.CrmUserSettingsN.AddCrmUserSettingsService;

public interface IAddCrmUserSettingsService
{
    Task<bool> AddCrmUserSettingsAsync(CrmUserSettings crmUserSettings);
}
