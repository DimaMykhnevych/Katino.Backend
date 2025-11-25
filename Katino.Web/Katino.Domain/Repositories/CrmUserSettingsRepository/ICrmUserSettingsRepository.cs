using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.CrmUserSettingsRepository;

public interface ICrmUserSettingsRepository : IRepository<CrmUserSettings>
{
    Task<CrmUserSettings> GetAppUserSettingsWithFullInfo(Guid appUserId);
}
