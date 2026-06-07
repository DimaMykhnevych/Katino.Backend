using Katino.Domain.Entities;
using Katino.Domain.Models.Pricing;

namespace Katino.Domain.Services.DiscountN.DiscountCalculationService;

public interface IDiscountCalculationService
{
    OrderPricingResult Calculate(IReadOnlyList<OrderPricingItem> items, IReadOnlyList<Discount> activeDiscounts);
}
