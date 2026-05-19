namespace Katino.Application.DTOs.Telegram;

public class TelegramSettingsDto
{
    public bool IsConfigured { get; set; }
    public string BotTokenMasked { get; set; }
    public List<TelegramChatConfigDto> ChatConfigs { get; set; } = new();
}
