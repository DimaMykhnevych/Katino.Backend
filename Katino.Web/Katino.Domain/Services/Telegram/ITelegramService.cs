using Katino.Domain.Models;

namespace Katino.Domain.Services.TelegramN;

public interface ITelegramService
{
    Task SendSewingReportNotificationAsync(List<SewedReport> report, string sewerName);
    Task SendTestMessageAsync();
    Task<TelegramBotInfo> ValidateBotTokenAsync(string token);
    Task<List<TelegramChatInfo>> GetAvailableChatsAsync(string token);
}
