using Katino.Application.DTOs.Telegram;
using Katino.Domain.Services.TelegramN;
using MediatR;

namespace Katino.Application.Queries.Telegram.ValidateTelegramBot;

public class ValidateTelegramBotQueryHandler : IRequestHandler<ValidateTelegramBotQuery, TelegramBotInfoDto>
{
    private readonly ITelegramService _telegramService;

    public ValidateTelegramBotQueryHandler(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    public async Task<TelegramBotInfoDto> Handle(ValidateTelegramBotQuery request, CancellationToken cancellationToken)
    {
        var info = await _telegramService.ValidateBotTokenAsync(request.Token);

        return new TelegramBotInfoDto
        {
            Id = info.Id,
            Username = info.Username,
            FirstName = info.FirstName
        };
    }
}
