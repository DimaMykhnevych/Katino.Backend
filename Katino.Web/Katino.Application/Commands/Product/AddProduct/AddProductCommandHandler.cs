using AutoMapper;
using Katino.Application.DTOs.Product;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductN.AddProduct;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddProductCommandHandler(
        IProductRepository productRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddProductCommandHandler));
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add product request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Product product = _mapper.Map<Product>(request);
            var addedProduct = await _productRepository.Insert(product);
            await _productRepository.Save();

            var addedProductWithCategory = await _productRepository.GetProductWithCategoryAsync(addedProduct.Id);
            return _mapper.Map<ProductDto>(addedProductWithCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product");
            return null;
        }
    }
}
