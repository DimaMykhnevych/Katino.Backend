using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.DiscountRepository;
using Katino.Domain.Services.DiscountN.AddDiscountService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.DiscountN;

public class AddDiscountService : IAddDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger _logger;

    public AddDiscountService(
        IDiscountRepository discountRepository,
        ILoggerFactory loggerFactory)
    {
        _discountRepository = discountRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddDiscountService));
    }

    public async Task<Discount> AddAsync(Discount discount)
    {
        _logger.LogInformation("Adding discount of type {DiscountType}", discount.Type);

        if (discount.Type == DiscountType.Global)
        {
            var activeDiscounts = await _discountRepository.GetActiveWithDetailsAsync();
            if (activeDiscounts.Any(d => d.Type == DiscountType.Global))
            {
                _logger.LogWarning("An active global discount already exists");
                return null;
            }
        }

        await _discountRepository.Insert(discount);
        await _discountRepository.Save();

        return await _discountRepository.GetWithDetailsAsync(discount.Id);
    }
}
