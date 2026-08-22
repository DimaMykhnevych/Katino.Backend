using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.TelegramN;
using System.Text;

namespace Katino.Infrastructure.Persistance.Services.TelegramN;

public class SewingReportNotifier : BaseNotifier, ISewingReportNotifier
{
    // TODO i18n
    private const string MessageHeader = "Звіт від швеї:";

    private readonly ITelegramService _telegramService;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderItemRepository _orderItemRepository;

    public SewingReportNotifier(
        ITelegramService telegramService,
        IProductVariantRepository productVariantRepository,
        IOrderItemRepository orderItemRepository)
    {
        _telegramService = telegramService;
        _productVariantRepository = productVariantRepository;
        _orderItemRepository = orderItemRepository;
    }

    public async Task NotifyAsync(List<SewedReport> report, string sewerName)
    {
        var message = await BuildMessageAsync(report, sewerName);
        await _telegramService.SendAsync(message, TelegramNotificationType.SewingReport);
    }

    private async Task<string> BuildMessageAsync(List<SewedReport> report, string sewerName)
    {
        StringBuilder sb = new();
        sb.AppendLine($"🧵 <b>{MessageHeader} {sewerName}</b>");
        sb.AppendLine($"📅 {DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)):dd.MM.yyyy HH:mm}");
        sb.AppendLine();

        foreach (var item in report.Where(r => r.ActualSewedQuantity > 0))
        {
            var pv = await _productVariantRepository.GetWithProductColorAndSize(item.ProductVariantId);
            var label = BuildProductLabel(pv, item.ProductVariantId);
            var metaStr = BuildProductMetaSuffix(pv);

            var orderNote = "";
            OrderItem orderItem = null;
            if (item.OrderItemId.HasValue)
            {
                orderItem = await _orderItemRepository.Get(item.OrderItemId.Value);
                orderNote = orderItem.IsCustomTailoring ? $" <i>[{CustomText}]</i>" : $" <i>[{ReturnText}]</i>";
            }

            sb.AppendLine($"• {label}{metaStr} — {item.ActualSewedQuantity} {QuantityText}{orderNote}");

            if (item.OrderItemId.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(orderItem?.Comment))
                {
                    sb.AppendLine($"  💬 <i>{orderItem.Comment}</i>");
                }
            }
        }

        return sb.ToString();
    }
}
