using Katino.Domain.Builders;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.DiscountRepository;
using Katino.Store.Application.DTOs.Products;
using Katino.Store.Application.Services.Discounts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Queries.Products.GetProductCard;

public class GetProductCardQueryHandler : IRequestHandler<GetProductCardQuery, ProductCardDto>
{
    private readonly IProductQueryBuilder _productQueryBuilder;
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger _logger;

    public GetProductCardQueryHandler(
        IProductQueryBuilder productQueryBuilder,
        IDiscountRepository discountRepository,
        ILoggerFactory loggerFactory)
    {
        _productQueryBuilder = productQueryBuilder;
        _discountRepository = discountRepository;
        _logger = loggerFactory.CreateLogger(nameof(GetProductCardQueryHandler));
    }

    public async Task<ProductCardDto> Handle(GetProductCardQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        _logger.LogInformation("Handling get product card request. ProductId = {productId}", request.Id);

        var product = await _productQueryBuilder
            .SetBaseQuery()
            .ApplyIdFilter(request.Id)
            .IncludeCategory()
            .IncludeVariantDetails()
            .Build()
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return null;
        }

        var activeDiscounts = await _discountRepository.GetActiveWithDetailsAsync();
        var (hasDiscount, discountPrice) = ProductDiscountResolver.Resolve(product, activeDiscounts);

        return new ProductCardDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = new ProductCardCategoryDto
            {
                Id = product.Category.Id,
                Name = product.Category.Name
            },
            Price = product.Price,
            HasDiscount = hasDiscount,
            DiscountPrice = discountPrice,
            Variants = [.. product.Variants
                .Where(v => v.Status != ProductStatus.Discontinued)
                .Select(v => new ProductCardVariantDto
                {
                    Id = v.Id,
                    Size = new ProductCardSizeDto { Id = v.Size.Id, Name = v.Size.Name },
                    Color = new ProductCardColorDto { Id = v.Color.Id, Name = v.Color.Name, HexCode = v.Color.HexCode },
                    Status = (ProductVariantStatusDto)v.Status,
                    Article = v.Article,
                    Photos = [.. v.Photos
                        .OrderBy(p => p.DisplayOrder)
                        .Select(p => new ProductCardPhotoDto
                        {
                            Id = p.Id,
                            PhotoUrl = p.PhotoUrl,
                            AltText = p.AltText,
                            DisplayOrder = p.DisplayOrder
                        })],
                    Measurements = [.. v.Measurements
                        .Select(m => new ProductCardMeasurementDto
                        {
                            Id = m.Id,
                            Value = m.Value,
                            MeasurementType = new ProductCardMeasurementTypeDto
                            {
                                Id = m.MeasurementType.Id,
                                Name = m.MeasurementType.Name,
                                Unit = m.MeasurementType.Unit
                            }
                        })]
                })]
        };
    }
}
