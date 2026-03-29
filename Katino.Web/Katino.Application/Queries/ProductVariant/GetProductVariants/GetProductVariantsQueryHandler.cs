using AutoMapper;
using Katino.Application.DTOs.ProductVariant;
using Katino.Domain.Builders;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.ProductVariantN.GetProductVariants;

public class GetProductVariantsQueryHandler : IRequestHandler<GetProductVariantsQuery, GetProductVariantDto>
{
    private readonly IProductVariantQueryBuilder _queryBuilder;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetProductVariantsQueryHandler(
        IProductVariantQueryBuilder queryBuilder,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _queryBuilder = queryBuilder;
        _logger = loggerFactory?.CreateLogger(nameof(GetProductVariantsQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetProductVariantDto> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get product variants");
        ArgumentNullException.ThrowIfNull(request);

        var productStatus = request.ProductStatus != null
            ? _mapper.Map<ProductStatus>(request.ProductStatus)
            : (ProductStatus?)null;

        _queryBuilder
            .SetBaseQuery()
            .ApplyNameFilter(request.ProductName)
            .ApplyCategoryFilter(request.CategoryId)
            .ApplyStatusFilter(productStatus);

        if (request.GetLastAddedProductVariant == true)
        {
            var last = await _queryBuilder.Build()
                .FirstOrDefaultAsync(cancellationToken);

            var mappedLast = _mapper.Map<ProductVariantDto>(last);

            return new GetProductVariantDto
            {
                ProductVariants = mappedLast == null ? [] : [mappedLast],
                ResultsAmount = mappedLast == null ? 0 : 1
            };
        }

        // TODO IN-MEMORY pagination, is okay for now, in future - rewrite!
        var allFiltered = await _queryBuilder.Build().ToListAsync(cancellationToken);

        var sorted = allFiltered
            .GroupBy(pv => pv.ProductId)
            .OrderByDescending(g => g.Max(pv => pv.CreatedAt))
            .SelectMany(g => g
                .OrderBy(pv => pv.Color.Name)
                .ThenBy(pv => SortHelper.GetSizeSortGroup(pv.Size.Name))
                .ThenBy(pv => SortHelper.GetSizeSortValue(pv.Size.Name))
                .ThenBy(pv => SortHelper.NormalizeSizeName(pv.Size.Name)))
            .ToList();

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var productVariants = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var productVariantDtos = _mapper
            .Map<IEnumerable<ProductVariantDto>>(productVariants)
            .ToList();

        return new GetProductVariantDto
        {
            ProductVariants = productVariantDtos,
            ResultsAmount = sorted.Count
        };
    }
}