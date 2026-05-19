using Katino.Domain.Enums;
using Katino.Domain.Models;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using Katino.Domain.Services.TelegramN;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Katino.Infrastructure.Persistance.Services.TelegramN;

public class TelegramService : ITelegramService
{
    private const string TestMessageText = "✅ Тестове повідомлення від Katino CRM. Telegram-сповіщення налаштовано успішно!";

    private readonly ITelegramSettingsRepository _telegramSettingsRepository;

    public TelegramService(ITelegramSettingsRepository telegramSettingsRepository)
    {
        _telegramSettingsRepository = telegramSettingsRepository;
    }

    public async Task SendAsync(string message, TelegramNotificationType notificationType)
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null || string.IsNullOrEmpty(settings.BotToken))
        {
            return;
        }

        var chatConfig = settings.ChatConfigs
            .FirstOrDefault(c => c.NotificationType == notificationType && c.NotificationsEnabled);

        if (chatConfig is null || string.IsNullOrEmpty(chatConfig.ChatId))
        {
            return;
        }

        var botClient = new TelegramBotClient(settings.BotToken);
        await botClient.SendMessage(ResolveChatId(chatConfig.ChatId), message, parseMode: ParseMode.Html);
    }

    public async Task SendTestMessageAsync()
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null || string.IsNullOrEmpty(settings.BotToken))
        {
            throw new InvalidOperationException("Bot token should be set");
        }

        if (settings.ChatConfigs.Count == 0)
        {
            throw new InvalidOperationException("At least one chat should be configured");
        }
            
        var botClient = new TelegramBotClient(settings.BotToken);

        foreach (var chatConfig in settings.ChatConfigs)
        {
            await botClient.SendMessage(ResolveChatId(chatConfig.ChatId), TestMessageText);
        }
    }

    public async Task<TelegramBotInfo> ValidateBotTokenAsync(string token)
    {
        var botClient = new TelegramBotClient(token);
        var me = await botClient.GetMe();

        return new TelegramBotInfo
        {
            Id = me.Id,
            Username = me.Username ?? string.Empty,
            FirstName = me.FirstName
        };
    }

    public async Task<List<TelegramChatInfo>> GetAvailableChatsAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            var settings = await _telegramSettingsRepository.GetSettingsAsync();
            token = settings?.BotToken ?? string.Empty;
        }

        if (string.IsNullOrEmpty(token))
        {
            return [];
        }

        var botClient = new TelegramBotClient(token);
        var updates = await botClient.GetUpdates(limit: 100);

        return updates
            .Select(u => u.Message?.Chat ?? u.ChannelPost?.Chat ?? u.MyChatMember?.Chat)
            .Where(c => c is not null && c.Type != ChatType.Private)
            .DistinctBy(c => c!.Id)
            .Select(c => new TelegramChatInfo
            {
                Id = c!.Id,
                Title = c.Title ?? c.Username ?? c.Id.ToString(),
                Type = c.Type.ToString()
            })
            .ToList();
    }

    private static ChatId ResolveChatId(string chatIdString)
    {
        return long.TryParse(chatIdString, out var numericId)
            ? new ChatId(numericId)
            : new ChatId(chatIdString);
    }
}
