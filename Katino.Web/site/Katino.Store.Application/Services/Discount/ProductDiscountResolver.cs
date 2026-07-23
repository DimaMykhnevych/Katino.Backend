using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Store.Application.Services.Discounts;

public static class ProductDiscountResolver
{
    public static (bool HasDiscount, decimal? DiscountPrice) Resolve(Product product, IReadOnlyList<Discount> activeDiscounts)
    {
        var productSpecific = activeDiscounts
            .Where(d => d.Type == DiscountType.ProductSpecific && d.DiscountProducts.Any(dp => dp.ProductId == product.Id))
            .ToList();

        Discount best;
        if (productSpecific.Count > 0)
        {
            best = productSpecific.MaxBy(d => EffectiveValue(d, product.Price));
        }
        else
        {
            var collectionIds = product.ProductCollections.Select(pc => pc.CollectionId).ToHashSet();
            best = activeDiscounts
                .Where(d => d.Type == DiscountType.Collection && d.DiscountCollections.Any(dc => collectionIds.Contains(dc.CollectionId)))
                .MaxBy(d => EffectiveValue(d, product.Price));
        }

        if (best is null)
        {
            return (false, null);
        }

        var discountAmount = CalculateDiscountAmount(best, product.Price);
        return (true, product.Price - discountAmount);
    }

    private static decimal EffectiveValue(Discount discount, decimal price) =>
        discount.ValueType == DiscountValueType.Percentage
            ? price * discount.Value / 100m
            : discount.Value;

    private static decimal CalculateDiscountAmount(Discount discount, decimal price) =>
        discount.ValueType == DiscountValueType.Percentage
            ? Math.Round(price * discount.Value / 100m, 2)
            : Math.Min(discount.Value, price);
}
