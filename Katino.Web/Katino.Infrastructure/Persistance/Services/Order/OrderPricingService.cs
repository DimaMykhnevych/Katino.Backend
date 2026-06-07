using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Models.Pricing;
using Katino.Domain.Repositories.DiscountRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.DiscountN.DiscountCalculationService;
using Katino.Domain.Services.OrderN.OrderPricingService;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class OrderPricingService : IOrderPricingService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IDiscountRepository _discountRepository;
    private readonly IDiscountCalculationService _discountCalculationService;

    public OrderPricingService(
        IProductVariantRepository productVariantRepository,
        IDiscountRepository discountRepository,
        IDiscountCalculationService discountCalculationService)
    {
        _productVariantRepository = productVariantRepository;
        _discountRepository = discountRepository;
        _discountCalculationService = discountCalculationService;
    }

    public async Task<OrderPricingResult> CalculateAsync(IReadOnlyList<OrderItem> orderItems, SaleType saleType)
    {
        var requests = orderItems.Select(i => new OrderPricingRequest
        {
            ProductVariantId = i.ProductVariantId,
            Quantity = i.Quantity,
            IsCustomTailoring = i.IsCustomTailoring,
        }).ToList();

        return await CalculateByVariantIdsAsync(requests, saleType);
    }

    public async Task<OrderPricingResult> CalculateByVariantIdsAsync(IReadOnlyList<OrderPricingRequest> requests, SaleType saleType)
    {
        var variantIds = requests.Select(r => r.ProductVariantId).ToList();

        var variants = await _productVariantRepository.GetManyWithProductAndCollectionAsync(variantIds);
        var variantMap = variants.ToDictionary(v => v.Id);

        var activeDiscounts = await _discountRepository.GetActiveWithDetailsAsync();

        var pricingItems = requests.Select(r =>
        {
            variantMap.TryGetValue(r.ProductVariantId, out var variant);
            var unitPrice = variant != null ? ResolvePrice(variant.Product, saleType) : 0m;
            var collectionIds = variant?.Product.ProductCollections
                .Select(pc => pc.CollectionId)
                .ToList() ?? [];

            return new OrderPricingItem
            {
                ProductVariantId = r.ProductVariantId,
                ProductId = variant!.ProductId,
                CollectionIds = collectionIds,
                UnitPrice = unitPrice,
                Quantity = r.Quantity,
                IsCustomTailoring = r.IsCustomTailoring,
            };
        }).ToList();

        return _discountCalculationService.Calculate(pricingItems, activeDiscounts);
    }

    private static decimal ResolvePrice(Product product, SaleType saleType) => saleType switch
    {
        SaleType.Drop => product.DropPrice,
        SaleType.Wholesale => product.WholesalePrice,
        _ => product.Price,
    };
}
