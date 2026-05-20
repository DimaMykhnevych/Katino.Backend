using Katino.Domain.Entities;

namespace Katino.Infrastructure.Persistance.Services.TelegramN;

public abstract class BaseNotifier
{
    // TODO i18n
    protected const string CustomText = "індивід.";
    protected const string QuantityText = "шт.";

    protected static string BuildProductLabel(ProductVariant pv, Guid fallbackId)
    {
        return pv?.Product?.Name is not null
            ? $"{pv.Product.Name} ({pv.Article})"
            : pv?.Article ?? fallbackId.ToString();
    }

    protected static string BuildProductMetaSuffix(ProductVariant pv)
    {
        var sizePart = pv?.Size?.Name;
        var colorPart = BuildColorPart(pv?.Color?.Name, pv?.Color?.HexCode);
        var meta = string.Join(" | ", new[] { sizePart, colorPart }.Where(p => p is not null));
        return meta.Length > 0 ? $" | {meta}" : "";
    }

    protected static string BuildColorPart(string name, string hex)
    {
        if (name is null)
        {
            return null;
        }

        var trimmed = hex?.TrimStart('#');
        return trimmed is not null ? $"{name} (#{trimmed})" : name;
    }
}
