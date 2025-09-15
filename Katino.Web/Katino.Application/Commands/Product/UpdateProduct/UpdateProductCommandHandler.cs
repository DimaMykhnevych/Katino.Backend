using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductN.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ILoggerFactory loggerFactory)
    {
        _productRepository = productRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateProductCommandHandler));
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
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
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating product");
            return false;
        }
    }
}
