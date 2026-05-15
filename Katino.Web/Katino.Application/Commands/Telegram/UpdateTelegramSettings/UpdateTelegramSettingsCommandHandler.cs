using Katino.Domain.Entities;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using MediatR;

namespace Katino.Application.Commands.Telegram.UpdateTelegramSettings;

public class UpdateTelegramSettingsCommandHandler : IRequestHandler<UpdateTelegramSettingsCommand, bool>
{
    private readonly ITelegramSettingsRepository _telegramSettingsRepository;

    public UpdateTelegramSettingsCommandHandler(ITelegramSettingsRepository telegramSettingsRepository)
    {
        _telegramSettingsRepository = telegramSettingsRepository;
    }

    public async Task<bool> Handle(UpdateTelegramSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null)
        {
            await _telegramSettingsRepository.Insert(new TelegramSettings
            {
                Id = Guid.NewGuid(),
                BotToken = request.BotToken,
                ChatId = request.ChatId,
                NotificationsEnabled = request.NotificationsEnabled
            });
        }
        else
        {
            if (request.BotToken is not null)
            {
                settings.BotToken = request.BotToken;
            }

            settings.ChatId = request.ChatId;
            settings.NotificationsEnabled = request.NotificationsEnabled;
            await _telegramSettingsRepository.Update(settings);
        }

        await _telegramSettingsRepository.Save();
        return true;
    }
}
