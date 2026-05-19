using Katino.Application.DTOs.Telegram;
using MediatR;

namespace Katino.Application.Commands.Telegram.UpdateTelegramSettings;

public class UpdateTelegramSettingsCommand : IRequest<bool>
{
    /// <summary>
    /// New bot token. If null — token is not changed (keeps existing one).
    /// </summary>
    public string BotToken { get; set; }
    public List<TelegramChatConfigDto> ChatConfigs { get; set; } = new();
}
