using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.TelegramN;
using System.Text;

namespace Katino.Infrastructure.Persistance.Services.TelegramN;

public class OrderRejectionNotifier : BaseNotifier, IOrderRejectionNotifier
{
    // TODO i18n
    private const string MessageHeader = "Відмова від замовлення";
    private const string InternetDocLabel = "Накладна";
    private const string ItemsLabel = "Товари";
    private const string RecipientLabel = "Отримувач";
    private const string CommentLabel = "Коментар";
    private const string CrmLinkText = "Відкрити в CRM";

    private readonly ITelegramService _telegramService;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderRecipientRepository _orderRecipientRepository;

    public OrderRejectionNotifier(
        ITelegramService telegramService,
        IProductVariantRepository productVariantRepository,
        IOrderRecipientRepository orderRecipientRepository)
    {
        _telegramService = telegramService;
        _productVariantRepository = productVariantRepository;
        _orderRecipientRepository = orderRecipientRepository;
    }

    public async Task NotifyAsync(Order order)
    {
        var message = await BuildMessageAsync(order);
        await _telegramService.SendAsync(message, TelegramNotificationType.OrderRejection);
    }

    private async Task<string> BuildMessageAsync(Order order)
    {
        StringBuilder sb = new();
        sb.AppendLine($"🔴 <b>{MessageHeader}</b>");
        sb.AppendLine($"📦 {InternetDocLabel}: <code>{order.InternetDocumentIntDocNumber}</code>");
        sb.AppendLine();

        sb.AppendLine($"<b>{ItemsLabel}:</b>");
        foreach (var item in order.OrderItems)
        {
            var pv = await _productVariantRepository.GetWithProductColorAndSize(item.ProductVariantId);
            var label = BuildProductLabel(pv, item.ProductVariantId);
            var metaStr = BuildProductMetaSuffix(pv);
            var customTag = item.IsCustomTailoring ? $" <i>[{CustomText}]</i>" : "";

            sb.AppendLine($"• {label}{metaStr} — {item.Quantity} {QuantityText}{customTag}");

            if (!string.IsNullOrWhiteSpace(item.Comment))
            {
                sb.AppendLine($"  💬 <i>{item.Comment}</i>");
            }
        }

        if (order.OrderRecipientId.HasValue)
        {
            var recipient = await _orderRecipientRepository.GetWithNpContactPersonAsync(order.OrderRecipientId.Value);
            if (recipient?.NpContactPerson is not null)
            {
                var cp = recipient.NpContactPerson;
                sb.AppendLine();
                sb.AppendLine($"<b>{RecipientLabel}:</b>");

                var fullName = string.Join(" ", new[] { cp.LastName, cp.FirstName, cp.MiddleName }
                    .Where(n => !string.IsNullOrWhiteSpace(n)));
                sb.AppendLine($"👤 {fullName}");
                sb.AppendLine($"📞 {cp.Phones}");

                if (!string.IsNullOrWhiteSpace(recipient.InstUrl))
                {
                    sb.AppendLine($"📸 {recipient.InstUrl}");
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(order.Comment))
        {
            sb.AppendLine();
            sb.AppendLine($"💬 <i>{CommentLabel}: {order.Comment}</i>");
        }

        if (!string.IsNullOrWhiteSpace(order.GeneralOrderInfo))
        {
            sb.AppendLine($"📝 <i>{order.GeneralOrderInfo}</i>");
        }

        sb.AppendLine();
        sb.AppendLine($"🔗 <a href=\"{AppUrlConstants.BaseFrontendUrl}orders/{order.Id}\">{CrmLinkText}</a>");

        return sb.ToString();
    }
}
