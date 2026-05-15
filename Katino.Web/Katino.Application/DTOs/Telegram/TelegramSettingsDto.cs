namespace Katino.Application.DTOs.Telegram;

public class TelegramSettingsDto
{
    public bool IsConfigured { get; set; }
    public string BotTokenMasked { get; set; }
    public string ChatId { get; set; }
    public bool NotificationsEnabled { get; set; }
}
