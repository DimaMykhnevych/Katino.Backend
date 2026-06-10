using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Models.Pricing;
using Katino.Domain.Services.DiscountN.DiscountCalculationService;

namespace Katino.Infrastructure.Persistance.Services.DiscountN;

public class DiscountCalculationService : IDiscountCalculationService
{
    public OrderPricingResult Calculate(IReadOnlyList<OrderPricingItem> items, IReadOnlyList<Discount> activeDiscounts)
    {
        var itemResults = new List<ItemPricingResult>(items.Count);
        var bundleCandidates = new HashSet<Guid>(); // ProductIds without a higher-priority discount

        var productSpecific = activeDiscounts.Where(d => d.Type == DiscountType.ProductSpecific).ToList();
        var collectionDiscounts = activeDiscounts.Where(d => d.Type == DiscountType.Collection).ToList();
        var globalDiscount = activeDiscounts.Where(d => d.Type == DiscountType.Global).MaxBy(d => d.Value);
        var bundleDiscounts = activeDiscounts.Where(d => d.Type == DiscountType.Bundle).ToList();

        foreach (var item in items)
        {
            var lineTotal = item.UnitPrice * item.Quantity;

            if (item.IsCustomTailoring)
            {
                itemResults.Add(new ItemPricingResult
                {
                    ProductVariantId = item.ProductVariantId,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal,
                    DiscountAmount = 0,
                    FinalLineTotal = lineTotal,
                });
                continue;
            }

            var appliedDiscount = FindProductSpecificDiscount(item, productSpecific)
                ?? FindCollectionDiscount(item, collectionDiscounts)
                ?? globalDiscount;

            if (appliedDiscount != null)
            {
                var discountAmount = CalculateDiscountAmount(appliedDiscount, item.UnitPrice, item.Quantity);
                itemResults.Add(new ItemPricingResult
                {
                    ProductVariantId = item.ProductVariantId,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal,
                    DiscountAmount = discountAmount,
                    FinalLineTotal = lineTotal - discountAmount,
                    DiscountId = appliedDiscount.Id,
                    AppliedDiscountType = appliedDiscount.Type,
                });
            }
            else
            {
                itemResults.Add(new ItemPricingResult
                {
                    ProductVariantId = item.ProductVariantId,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal,
                    DiscountAmount = 0,
                    FinalLineTotal = lineTotal,
                });
                bundleCandidates.Add(item.ProductId);
            }
        }

        // Bundle discounts: all bundle products must be present in order AND all must be bundle candidates
        var productIdsInOrder = items.Select(i => i.ProductId).ToHashSet();
        foreach (var bundle in bundleDiscounts)
        {
            var bundleProductIds = bundle.BundleProducts.Select(bp => bp.ProductId).ToHashSet();

            var allInOrder = bundleProductIds.All(pid => productIdsInOrder.Contains(pid));
            var allAreCandidates = bundleProductIds.All(pid => bundleCandidates.Contains(pid));

            if (!allInOrder || !allAreCandidates)
            {
                continue;
            }

            // Mark bundle products as having the bundle discount applied
            foreach (var result in itemResults)
            {
                var item = items.First(i => i.ProductVariantId == result.ProductVariantId);
                if (bundleProductIds.Contains(item.ProductId))
                {
                    result.DiscountId = bundle.Id;
                    result.AppliedDiscountType = DiscountType.Bundle;
                }
            }

            // The bundle fixed amount is distributed evenly across bundle items for record-keeping,
            // but the total deducted is exactly bundle.Value multiplied by the number of complete sets
            var bundleItems = itemResults
                .Where(r => r.DiscountId == bundle.Id && r.AppliedDiscountType == DiscountType.Bundle)
                .ToList();

            var quantityByProduct = bundleItems
                .GroupBy(r => items.First(i => i.ProductVariantId == r.ProductVariantId).ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(r => items.First(i => i.ProductVariantId == r.ProductVariantId).Quantity));

            var setsCount = bundleProductIds.Min(pid => quantityByProduct.GetValueOrDefault(pid, 0));

            DistributeBundleDiscount(bundle.Value * setsCount, bundleItems);

            // Bundle products are no longer candidates for other bundles
            foreach (var pid in bundleProductIds)
            {
                bundleCandidates.Remove(pid);
            }
        }

        var baseTotal = itemResults.Sum(r => r.LineTotal);
        var totalDiscount = itemResults.Sum(r => r.DiscountAmount);

        return new OrderPricingResult
        {
            BaseTotal = baseTotal,
            TotalDiscount = totalDiscount,
            FinalTotal = baseTotal - totalDiscount,
            ItemResults = itemResults,
        };
    }

    private static Discount? FindProductSpecificDiscount(OrderPricingItem item, List<Discount> candidates)
    {
        return candidates
            .Where(d => d.DiscountProducts.Any(dp => dp.ProductId == item.ProductId))
            .MaxBy(d => EffectiveValue(d, item.UnitPrice));
    }

    private static Discount? FindCollectionDiscount(OrderPricingItem item, List<Discount> candidates)
    {
        return candidates
            .Where(d => d.DiscountCollections.Any(dc => item.CollectionIds.Contains(dc.CollectionId)))
            .MaxBy(d => EffectiveValue(d, item.UnitPrice));
    }

    private static decimal EffectiveValue(Discount discount, decimal unitPrice) =>
        discount.ValueType == DiscountValueType.Percentage
            ? unitPrice * discount.Value / 100m
            : discount.Value;

    private static decimal CalculateDiscountAmount(Discount discount, decimal unitPrice, int quantity) =>
        discount.ValueType == DiscountValueType.Percentage
            ? Math.Round(unitPrice * quantity * discount.Value / 100m, 2)
            : Math.Min(discount.Value * quantity, unitPrice * quantity);

    private static void DistributeBundleDiscount(decimal bundleTotal, List<ItemPricingResult> bundleItems)
    {
        if (bundleItems.Count == 0)
        {
            return;
        }

        var totalLineValue = bundleItems.Sum(i => i.LineTotal);
        var distributed = 0m;

        for (var i = 0; i < bundleItems.Count - 1; i++)
        {
            var share = totalLineValue > 0
                ? Math.Round(bundleTotal * bundleItems[i].LineTotal / totalLineValue, 2)
                : Math.Round(bundleTotal / bundleItems.Count, 2);

            bundleItems[i].DiscountAmount = share;
            bundleItems[i].FinalLineTotal = bundleItems[i].LineTotal - share;
            distributed += share;
        }

        // Last item gets the remainder to avoid rounding drift
        var last = bundleItems[^1];
        last.DiscountAmount = bundleTotal - distributed;
        last.FinalLineTotal = last.LineTotal - last.DiscountAmount;
    }
}
