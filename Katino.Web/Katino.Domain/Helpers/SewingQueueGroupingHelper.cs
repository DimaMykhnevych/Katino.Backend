using Katino.Domain.Models;

namespace Katino.Domain.Helpers;

public static class SewingQueueGroupingHelper
{
    public static Dictionary<DateTime, List<SewingQueueItem>> GroupByDate(IEnumerable<(DateTime Date, SewingQueueItem Item)> entries)
    {
        return entries
            .GroupBy(e => e.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => Group(g.Select(e => e.Item)));
    }

    public static List<SewingQueueItem> Group(IEnumerable<SewingQueueItem> items)
    {
        // Custom-tailoring and incoming-return items are tied to one specific order item each,
        // so they must stay separate rather than being merged into a per-ProductVariantId total.
        var ungrouped = items
            .Where(i => i.IsCustomTailoring || i.IsIncomingReturn)
            .ToList();

        var grouped = items
            .Where(i => !i.IsCustomTailoring && !i.IsIncomingReturn)
            .GroupBy(i => i.ProductVariantId)
            .Select(g =>
            {
                var first = g.First();
                return new SewingQueueItem
                {
                    ProductVariantId = g.Key,
                    ProductVariant = first.ProductVariant,
                    QuantityToProduce = g.Sum(i => i.QuantityToProduce),
                    IsCustomTailoring = false,
                    IsIncomingReturn = false,
                    Comment = null,
                    OrderItemId = null
                };
            })
            .ToList();

        return grouped.Concat(ungrouped)
            .OrderBy(i => i.ProductVariant.Product.Name)
            .ThenBy(i => i.ProductVariant.Color.Name)
            .ThenBy(i => SortHelper.GetSizeSortGroup(i.ProductVariant.Size.Name))
            .ThenBy(i => SortHelper.GetSizeSortValue(i.ProductVariant.Size.Name))
            .ThenBy(i => SortHelper.NormalizeSizeName(i.ProductVariant.Size.Name))
            .ToList();
    }
}
