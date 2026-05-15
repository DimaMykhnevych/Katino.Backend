namespace Katino.Domain.Entities;

public class TelegramSettings
{
    public Guid Id { get; set; }
    public string BotToken { get; set; }
    public string ChatId { get; set; }
    public bool NotificationsEnabled { get; set; }
}
