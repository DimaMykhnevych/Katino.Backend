using AutoMapper;
using Katino.Application.DTOs.Product;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.ProductN.GetProducts;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductDto>
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetProductsQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetProductDto> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get products");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<Product> products = _katinoDbContext.Products
            .Include(p => p.Category)
            .AsNoTracking();
        if (!string.IsNullOrEmpty(request.Name))
        {
            products = products.Where(p => p.Name.Contains(request.Name));
        }

        var resultProducts = await products.ToListAsync();
        IEnumerable<ProductDto> productDtos = 
            _mapper.Map<IEnumerable<ProductDto>>(resultProducts);

        return new GetProductDto()
        {
            Products = productDtos,
            ResultsAmount = productDtos.Count()
        };
    }
}
