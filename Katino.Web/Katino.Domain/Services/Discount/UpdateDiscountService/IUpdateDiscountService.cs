using Katino.Domain.Entities;

namespace Katino.Domain.Services.DiscountN.UpdateDiscountService;

public interface IUpdateDiscountService
{
    Task<Discount> UpdateAsync(Discount discountUpdate, List<Guid> productIds, List<Guid> collectionIds, List<Guid> bundleProductIds);
}
