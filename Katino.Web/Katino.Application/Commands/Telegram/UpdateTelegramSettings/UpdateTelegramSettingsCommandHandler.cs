using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using MediatR;

namespace Katino.Application.Commands.Telegram.UpdateTelegramSettings;

public class UpdateTelegramSettingsCommandHandler : IRequestHandler<UpdateTelegramSettingsCommand, bool>
{
    private readonly ITelegramSettingsRepository _telegramSettingsRepository;
    private readonly IMapper _mapper;

    public UpdateTelegramSettingsCommandHandler(ITelegramSettingsRepository telegramSettingsRepository, IMapper mapper)
    {
        _telegramSettingsRepository = telegramSettingsRepository;
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateTelegramSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null)
        {
            var newSettings = new TelegramSettings
            {
                Id = Guid.NewGuid(),
                BotToken = request.BotToken
            };

            foreach (var dto in request.ChatConfigs)
            {
                var chatConfig = _mapper.Map<TelegramChatConfig>(dto);
                chatConfig.TelegramSettingsId = newSettings.Id;
                newSettings.ChatConfigs.Add(chatConfig);
            }

            await _telegramSettingsRepository.Insert(newSettings);
        }
        else
        {
            if (request.BotToken is not null)
            {
                settings.BotToken = request.BotToken;
            }

            settings.ChatConfigs.Clear();
            foreach (var dto in request.ChatConfigs)
            {
                var chatConfig = _mapper.Map<TelegramChatConfig>(dto);
                chatConfig.TelegramSettingsId = settings.Id;
                settings.ChatConfigs.Add(chatConfig);
            }

            await _telegramSettingsRepository.Update(settings);
        }

        await _telegramSettingsRepository.Save();
        return true;
    }
}
