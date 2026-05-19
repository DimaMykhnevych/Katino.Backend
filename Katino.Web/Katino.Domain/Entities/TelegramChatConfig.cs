using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class TelegramChatConfig
{
    public Guid Id { get; set; }
    public Guid TelegramSettingsId { get; set; }
    public TelegramSettings TelegramSettings { get; set; }
    public string ChatId { get; set; }
    public string ChatName { get; set; }
    public TelegramNotificationType NotificationType { get; set; }
    public bool NotificationsEnabled { get; set; }
}
