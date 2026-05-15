using MediatR;

namespace Katino.Application.Commands.Telegram.UpdateTelegramSettings;

public class UpdateTelegramSettingsCommand : IRequest<bool>
{
    /// <summary>
    /// New bot token. If null — token is not changed (keeps existing one).
    /// </summary>
    public string BotToken { get; set; }
    public string ChatId { get; set; }
    public bool NotificationsEnabled { get; set; }
}
