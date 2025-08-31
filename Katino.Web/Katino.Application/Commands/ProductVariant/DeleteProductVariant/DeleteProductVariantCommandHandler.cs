using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductVariantRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductVariantN.DeleteProductVariant;

public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, bool>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly ILogger _logger;

    public DeleteProductVariantCommandHandler(
        IProductVariantRepository productVariantRepository,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteProductVariantCommandHandler));
    }

    public async Task<bool> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete product variant request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            ProductVariant productVariantFromDb = await _productVariantRepository.Get(request.Id);
            if (productVariantFromDb == null)
            {
                throw new ArgumentException($"Product variant with id {request.Id} doesn't exist");
            }

            _productVariantRepository.Delete(productVariantFromDb);
            await _productVariantRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting product variant");
            return false;
        }
    }
}
