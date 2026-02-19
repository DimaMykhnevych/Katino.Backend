using AutoMapper;
using Katino.Application.DTOs.ProductVariant;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.ProductVariantN.GetProductVariants;

public class GetProductVariantsQueryHandler : IRequestHandler<GetProductVariantsQuery, GetProductVariantDto>
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetProductVariantsQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetProductVariantsQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetProductVariantDto> Handle(GetProductVariantsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get product variants");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<ProductVariant> productVariants = _katinoDbContext.ProductVariants
            .Include(p => p.Product)
            .ThenInclude(p => p.Category)
            .AsNoTracking()
            .Include(pv => pv.Size)
            .AsNoTracking()
            .Include(pv => pv.Color)
            .AsNoTracking()
            .Include(pv => pv.Photos)
            .AsNoTracking()
            .Include(pv => pv.Measurements)
            .ThenInclude(pvm => pvm.MeasurementType)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt);

        if (!string.IsNullOrWhiteSpace(request.ProductName))
        {
            productVariants = productVariants
                .Where(pv => pv.Product.Name.ToLower().Contains(request.ProductName.ToLower()));
        }

        if (request.CategoryId != null)
        {
            productVariants = productVariants
                .Where(pv => pv.Product.Category.Id == request.CategoryId);
        }

        if (request.ProductStatus != null)
        {
            var productStatus = _mapper.Map<ProductStatus>(request.ProductStatus);
            productVariants = productVariants
                .Where(pv => pv.Status == productStatus);
        }

        if (request.GetLastAddedProductVariant != null && request.GetLastAddedProductVariant.Value)
        {
            var lastAddedProductVariant = await productVariants
                .OrderByDescending(pv => pv.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var mappedProductVariant = _mapper.Map<ProductVariantDto>(lastAddedProductVariant);

            return new GetProductVariantDto
            {
                ProductVariants = mappedProductVariant == null ? [] : [mappedProductVariant],
                ResultsAmount = mappedProductVariant == null ? 0 : 1
            };
        }

        var resultProductVariants = await productVariants.ToListAsync(cancellationToken);
        List<ProductVariantDto> productVariantDtos =
            _mapper.Map<IEnumerable<ProductVariantDto>>(resultProductVariants)
                .ToList();

        return new GetProductVariantDto
        {
            ProductVariants = productVariantDtos,
            ResultsAmount = productVariantDtos.Count
        };
    }
}
