namespace Katino.Application.DTOs.Telegram;

public class TelegramChatConfigDto
{
    public string ChatId { get; set; }
    public string ChatName { get; set; }
    public TelegramNotificationTypeDto NotificationType { get; set; }
    public bool NotificationsEnabled { get; set; }
}
