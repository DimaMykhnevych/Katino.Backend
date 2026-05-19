using Katino.Domain.Enums;
using Katino.Domain.Models;

namespace Katino.Domain.Services.TelegramN;

public interface ITelegramService
{
    Task SendAsync(string message, TelegramNotificationType notificationType);
    Task SendTestMessageAsync();
    Task<TelegramBotInfo> ValidateBotTokenAsync(string token);
    Task<List<TelegramChatInfo>> GetAvailableChatsAsync(string token);
}
