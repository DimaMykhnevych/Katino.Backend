using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Models.Pricing;

namespace Katino.Domain.Services.OrderN.OrderPricingService;

public interface IOrderPricingService
{
    Task<OrderPricingResult> CalculateAsync(IReadOnlyList<OrderItem> orderItems, SaleType saleType);
    Task<OrderPricingResult> CalculateByVariantIdsAsync(IReadOnlyList<OrderPricingRequest> requests, SaleType saleType);
}
