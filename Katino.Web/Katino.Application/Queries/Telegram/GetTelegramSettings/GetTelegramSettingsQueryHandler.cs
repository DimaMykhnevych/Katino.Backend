using Katino.Application.DTOs.Telegram;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using MediatR;

namespace Katino.Application.Queries.Telegram.GetTelegramSettings;

public class GetTelegramSettingsQueryHandler : IRequestHandler<GetTelegramSettingsQuery, TelegramSettingsDto>
{
    private readonly ITelegramSettingsRepository _telegramSettingsRepository;

    public GetTelegramSettingsQueryHandler(ITelegramSettingsRepository telegramSettingsRepository)
    {
        _telegramSettingsRepository = telegramSettingsRepository;
    }

    public async Task<TelegramSettingsDto> Handle(GetTelegramSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null)
            return new TelegramSettingsDto { IsConfigured = false };

        return new TelegramSettingsDto
        {
            IsConfigured = !string.IsNullOrEmpty(settings.BotToken) && !string.IsNullOrEmpty(settings.ChatId),
            BotTokenMasked = settings.BotToken is not null ? new string('*', settings.BotToken.Length) : null,
            ChatId = settings.ChatId,
            NotificationsEnabled = settings.NotificationsEnabled
        };
    }
}
