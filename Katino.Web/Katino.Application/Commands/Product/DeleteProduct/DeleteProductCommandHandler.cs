using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductN.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        ILoggerFactory loggerFactory)
    {
        _productRepository = productRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteProductCommandHandler));
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete product request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Product productFromDb = await _productRepository.Get(request.Id);
            if (productFromDb == null)
            {
                throw new ArgumentException($"Product with id {request.Id} doesn't exist");
            }

            _productRepository.Delete(productFromDb);
            await _productRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting product");
            return false;
        }
    }
}

