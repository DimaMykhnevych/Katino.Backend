using AutoMapper;
using Katino.Application.DTOs.Product;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductN.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _productRepository = productRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateProductCommandHandler));
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update product request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Product productFromDb = await _productRepository.Get(request.Id);
            productFromDb.Name = request.Name;
            productFromDb.CategoryId = request.CategoryId;
            productFromDb.CostPrice = request.CostPrice;
            productFromDb.WholesalePrice = request.WholesalePrice;
            productFromDb.DropPrice = request.DropPrice;
            productFromDb.Price = request.Price;

            await _productRepository.Update(productFromDb);
            await _productRepository.Save();

            var updatedProductWithCategory = await _productRepository.GetProductWithCategoryAsync(productFromDb.Id);
            return _mapper.Map<ProductDto>(updatedProductWithCategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating product");
            return null;
        }
    }
}
