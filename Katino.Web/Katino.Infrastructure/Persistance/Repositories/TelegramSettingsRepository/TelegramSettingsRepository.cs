using Katino.Domain.Entities;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.TelegramSettingsRepository;

public class TelegramSettingsRepository : Repository<TelegramSettings>, ITelegramSettingsRepository
{
    public TelegramSettingsRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<TelegramSettings> GetSettingsAsync()
    {
        return await context.TelegramSettings.FirstOrDefaultAsync();
    }
}
