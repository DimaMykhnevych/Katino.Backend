using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using Katino.Domain.Services.TelegramN;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Katino.Infrastructure.Persistance.Services.TelegramN;

public class TelegramService : ITelegramService
{
    // TODO localization in future
    private const string MessageHeader = "Звіт від швеї:";
    private const string CustomText = "індивід.";
    private const string QuantityText = "шт.";
    private const string TestMessageText = "✅ Тестове повідомлення від Katino CRM. Telegram-сповіщення налаштовано успішно!";

    private readonly ITelegramSettingsRepository _telegramSettingsRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderItemRepository _orderItemRepository;

    public TelegramService(
        ITelegramSettingsRepository telegramSettingsRepository,
        IProductVariantRepository productVariantRepository,
        IOrderItemRepository orderItemRepository)
    {
        _telegramSettingsRepository = telegramSettingsRepository;
        _productVariantRepository = productVariantRepository;
        _orderItemRepository = orderItemRepository;
    }

    public async Task SendSewingReportNotificationAsync(List<SewedReport> report, string sewerName)
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null || !settings.NotificationsEnabled ||
            string.IsNullOrEmpty(settings.BotToken) || string.IsNullOrEmpty(settings.ChatId))
        {
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine($"🧵 <b>{MessageHeader} {sewerName}</b>");
        sb.AppendLine($"📅 {DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)):dd.MM.yyyy HH:mm}");
        sb.AppendLine();

        foreach (var item in report.Where(r => r.ActualSewedQuantity > 0))
        {
            var pv = await _productVariantRepository.GetWithProductColorAndSize(item.ProductVariantId);
            var label = pv?.Product?.Name is not null
                ? $"{pv.Product.Name} ({pv.Article})"
                : pv?.Article ?? item.ProductVariantId.ToString();

            var sizePart = pv?.Size?.Name;
            var colorPart = BuildColorPart(pv?.Color?.Name, pv?.Color?.HexCode);
            var meta = string.Join(" | ", new[] { sizePart, colorPart }.Where(p => p is not null));
            var metaStr = meta.Length > 0 ? $" | {meta}" : "";

            var orderNote = item.OrderItemId.HasValue ? $" <i>[{CustomText}]</i>" : "";
            sb.AppendLine($"• {label}{metaStr} — {item.ActualSewedQuantity} {QuantityText}{orderNote}");

            if (item.OrderItemId.HasValue)
            {
                var orderItem = await _orderItemRepository.Get(item.OrderItemId.Value);
                if (!string.IsNullOrWhiteSpace(orderItem?.Comment))
                {
                    sb.AppendLine($"  💬 <i>{orderItem.Comment}</i>");
                }
            }
        }

        var chatId = ResolveChatId(settings.ChatId);
        var botClient = new TelegramBotClient(settings.BotToken);
        await botClient.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Html);
    }

    public async Task SendTestMessageAsync()
    {
        var settings = await _telegramSettingsRepository.GetSettingsAsync();

        if (settings is null || string.IsNullOrEmpty(settings.BotToken) || string.IsNullOrEmpty(settings.ChatId))
            throw new InvalidOperationException("Bot token and chat id should be set");

        var chatId = ResolveChatId(settings.ChatId);
        var botClient = new TelegramBotClient(settings.BotToken);
        await botClient.SendMessage(chatId, TestMessageText);
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

    private static string BuildColorPart(string name, string hex)
    {
        if (name is null) return null;
        var trimmed = hex?.TrimStart('#');
        return trimmed is not null ? $"{name} (#{trimmed})" : name;
    }

    private static ChatId ResolveChatId(string chatIdString)
    {
        return long.TryParse(chatIdString, out var numericId)
            ? new ChatId(numericId)
            : new ChatId(chatIdString);
    }
}
