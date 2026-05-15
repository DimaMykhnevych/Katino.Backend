using Katino.Application.DTOs.Telegram;
using MediatR;

namespace Katino.Application.Queries.Telegram.ValidateTelegramBot;

public class ValidateTelegramBotQuery : IRequest<TelegramBotInfoDto>
{
    public string Token { get; set; }
}
