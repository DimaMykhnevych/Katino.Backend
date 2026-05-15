using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.TelegramSettingsRepository;

public interface ITelegramSettingsRepository : IRepository<TelegramSettings>
{
    Task<TelegramSettings> GetSettingsAsync();
}
