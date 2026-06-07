using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.DiscountRepository;
using Katino.Domain.Services.DiscountN.UpdateDiscountService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.DiscountN;

public class UpdateDiscountService : IUpdateDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;

    public UpdateDiscountService(
        IDiscountRepository discountRepository,
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory)
    {
        _discountRepository = discountRepository;
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateDiscountService));
    }

    public async Task<Discount> UpdateAsync(Discount discountUpdate, List<Guid> productIds, List<Guid> collectionIds, List<Guid> bundleProductIds)
    {
        _logger.LogInformation("Updating discount {DiscountId}", discountUpdate.Id);

        var discount = await _discountRepository.Get(discountUpdate.Id);
        if (discount == null)
        {
            _logger.LogWarning("Discount {DiscountId} not found", discountUpdate.Id);
            return null;
        }

        discount.Name = discountUpdate.Name;
        discount.ValueType = discountUpdate.ValueType;
        discount.Value = discountUpdate.Value;
        discount.IsActive = discountUpdate.IsActive;
        discount.StartDate = discountUpdate.StartDate;
        discount.EndDate = discountUpdate.EndDate;

        var oldProducts = await _katinoDbContext.DiscountProducts
            .Where(dp => dp.DiscountId == discount.Id)
            .ToListAsync();
        _katinoDbContext.DiscountProducts.RemoveRange(oldProducts);

        var oldCollections = await _katinoDbContext.DiscountCollections
            .Where(dc => dc.DiscountId == discount.Id)
            .ToListAsync();
        _katinoDbContext.DiscountCollections.RemoveRange(oldCollections);

        var oldBundleProducts = await _katinoDbContext.DiscountBundleProducts
            .Where(dbp => dbp.DiscountId == discount.Id)
            .ToListAsync();
        _katinoDbContext.DiscountBundleProducts.RemoveRange(oldBundleProducts);

        switch (discount.Type)
        {
            case DiscountType.ProductSpecific:
                _katinoDbContext.DiscountProducts.AddRange(
                    productIds.Select(id => new DiscountProduct { DiscountId = discount.Id, ProductId = id }));
                break;
            case DiscountType.Collection:
                _katinoDbContext.DiscountCollections.AddRange(
                    collectionIds.Select(id => new DiscountCollection { DiscountId = discount.Id, CollectionId = id }));
                break;
            case DiscountType.Bundle:
                _katinoDbContext.DiscountBundleProducts.AddRange(
                    bundleProductIds.Select(id => new DiscountBundleProduct { DiscountId = discount.Id, ProductId = id }));
                break;
        }

        await _discountRepository.Update(discount);
        await _discountRepository.Save();

        return await _discountRepository.GetWithDetailsAsync(discount.Id);
    }
}
