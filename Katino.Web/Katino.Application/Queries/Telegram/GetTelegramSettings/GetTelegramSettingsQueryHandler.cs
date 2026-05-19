using AutoMapper;
using Katino.Application.DTOs.Telegram;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using MediatR;

namespace Katino.Application.Queries.Telegram.GetTelegramSettings;

public class GetTelegramSettingsQueryHandler : IRequestHandler<GetTelegramSettingsQuery, TelegramSettingsDto>
{
    private readonly ITelegramSettingsRepository _telegramSettingsRepository;
    private readonly IMapper _mapper;

    public GetTelegramSettingsQueryHandler(ITelegramSettingsRepository telegramSettingsRepository, IMapper mapper)
    {
        _telegramSettingsRepository = telegramSettingsRepository;
        _mapper = mapper;
    }

    public async Task<TelegramSettingsDto> Handle(GetTelegramSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null)
        {
            return new TelegramSettingsDto { IsConfigured = false };
        }

        var chatConfigs = _mapper.Map<List<TelegramChatConfigDto>>(settings.ChatConfigs);

        return new TelegramSettingsDto
        {
            IsConfigured = !string.IsNullOrEmpty(settings.BotToken) && settings.ChatConfigs.Count > 0,
            BotTokenMasked = settings.BotToken is not null ? new string('*', settings.BotToken.Length) : null,
            ChatConfigs = chatConfigs
        };
    }
}
