using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.ProductVariantRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductVariantN.AddProductVariant;

public class AddProductVariantCommandHandler : IRequestHandler<AddProductVariantCommand, bool>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddProductVariantCommandHandler(
        IProductVariantRepository productVariantRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _productVariantRepository = productVariantRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddProductVariantCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(AddProductVariantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add product variant request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            ProductVariant productVariant = _mapper.Map<ProductVariant>(request.ProductVariant);
            if (productVariant.AvailableQuantity == 0 && productVariant.Status != ProductStatus.Discontinued)
            {
                productVariant.Status = ProductStatus.OnOrder;
            }

            await _productVariantRepository.Insert(productVariant);
            await _productVariantRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product variant");
            return false;
        }
    }
}
