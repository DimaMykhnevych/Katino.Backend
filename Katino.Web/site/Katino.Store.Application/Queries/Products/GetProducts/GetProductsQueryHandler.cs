using Katino.Domain.Builders;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.DiscountRepository;
using Katino.Store.Application.DTOs.Products;
using Katino.Store.Application.Services.Discounts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Queries.Products.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsDto>
{
    private readonly IProductQueryBuilder _productQueryBuilder;
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger _logger;

    public GetProductsQueryHandler(
        IProductQueryBuilder productQueryBuilder,
        IDiscountRepository discountRepository,
        ILoggerFactory loggerFactory)
    {
        _productQueryBuilder = productQueryBuilder;
        _discountRepository = discountRepository;
        _logger = loggerFactory.CreateLogger(nameof(GetProductsQueryHandler));
    }

    public async Task<GetProductsDto> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("Handling get products request. Search = {search}, Page = {page}", request.Search, request.Page);

        int totalCount = await _productQueryBuilder
            .SetBaseQuery()
            .ApplySearch(request.Search)
            .ApplyCategoryFilter(request.CategoryIds)
            .ApplyDiscountFilter(request.ReturnSpecificDiscountProducts)
            .Build()
            .CountAsync(cancellationToken);

        var products = await _productQueryBuilder
            .SetBaseQuery()
            .ApplySearch(request.Search)
            .ApplyCategoryFilter(request.CategoryIds)
            .ApplyDiscountFilter(request.ReturnSpecificDiscountProducts)
            .ApplyPaging(request.Page, request.PageSize)
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

        return new GetProductsDto
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
