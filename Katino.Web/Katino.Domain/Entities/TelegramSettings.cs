namespace Katino.Domain.Entities;

public class TelegramSettings
{
    public Guid Id { get; set; }
    public string BotToken { get; set; }
    public ICollection<TelegramChatConfig> ChatConfigs { get; set; } = new List<TelegramChatConfig>();
}
