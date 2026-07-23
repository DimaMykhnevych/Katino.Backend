using Katino.Domain.Builders;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.DiscountRepository;
using Katino.Store.Application.DTOs.Products;
using Katino.Store.Application.Services.Discounts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Queries.Products.GetRecentProducts;

public class GetRecentProductsQueryHandler : IRequestHandler<GetRecentProductsQuery, IEnumerable<ProductListItemDto>>
{
    private const string CacheKey = "recent-products";
    private const int PageSize = 20;

    private readonly IProductQueryBuilder _productQueryBuilder;
    private readonly IDiscountRepository _discountRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;

    public GetRecentProductsQueryHandler(
        IProductQueryBuilder productQueryBuilder,
        IDiscountRepository discountRepository,
        IMemoryCache memoryCache,
        ILoggerFactory loggerFactory)
    {
        _productQueryBuilder = productQueryBuilder;
        _discountRepository = discountRepository;
        _memoryCache = memoryCache;
        _logger = loggerFactory.CreateLogger(nameof(GetRecentProductsQueryHandler));
    }

    public async Task<IEnumerable<ProductListItemDto>> Handle(GetRecentProductsQuery request, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(CacheKey, out IEnumerable<ProductListItemDto> cached))
        {
            _logger.LogInformation("Returning recent products from cache");
            return cached;
        }

        _logger.LogInformation("Fetching recent products from database");

        var products = await _productQueryBuilder
            .SetBaseQuery()
            .ApplyPaging(1, PageSize)
            .Build()
            .ToListAsync(cancellationToken);

        var activeDiscounts = await _discountRepository.GetActiveWithDetailsAsync();

        var items = products.Select(p =>
        {
            var (hasDiscount, discountPrice) = ProductDiscountResolver.Resolve(p, activeDiscounts);

            return new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                Price = p.Price,
                HasDiscount = hasDiscount,
                DiscountPrice = discountPrice,
                PhotoUrl = p.Variants
                    .FirstOrDefault(v => v.Status != ProductStatus.Discontinued)?.Photos
                    .OrderBy(ph => ph.DisplayOrder)
                    .FirstOrDefault()?.PhotoUrl,
                Colors = [.. p.Variants
                    .Where(v => v.Status != ProductStatus.Discontinued)
                    .Select(v => v.Color)
                    .DistinctBy(c => c.Id)
                    .Select(c => new ProductColorDto { Name = c.Name, HexCode = c.HexCode })]
            };
        }).ToList();

        _memoryCache.Set(CacheKey, items, TimeSpan.FromMinutes(10));

        return items;
    }
}
