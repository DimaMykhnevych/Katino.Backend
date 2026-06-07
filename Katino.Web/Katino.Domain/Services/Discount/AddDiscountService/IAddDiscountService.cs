using Katino.Domain.Entities;

namespace Katino.Domain.Services.DiscountN.AddDiscountService;

public interface IAddDiscountService
{
    Task<Discount> AddAsync(Discount discount);
}
