using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.ProductPhotoRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.AzureStorage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ProductVariantN.AddProductVariant;

public class AddProductVariantCommandHandler : IRequestHandler<AddProductVariantCommand, bool>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IProductPhotoRepository _productPhotoRepository;
    private readonly IAzureStorageService _azureStorageService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddProductVariantCommandHandler(
        IProductVariantRepository productVariantRepository,
        IProductPhotoRepository productPhotoRepository,
        IAzureStorageService azureStorageService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _productVariantRepository = productVariantRepository;
        _productPhotoRepository = productPhotoRepository;
        _azureStorageService = azureStorageService;
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
            if (productVariant.QuantityInStock <= 0 && productVariant.Status != ProductStatus.Discontinued)
            {
                productVariant.Status = ProductStatus.OnOrder;
            }

            var addedProductVariant = await _productVariantRepository.Insert(productVariant);
            await _productVariantRepository.Save();

            if (request.ProductVariant.Photos != null && request.ProductVariant.Photos.Count > 0)
            {
                var photoUrls = await _azureStorageService
                    .UploadPhotosAsync(addedProductVariant.Id, request.ProductVariant.Photos);

                for (int i = 0; i < photoUrls.Count; i++)
                {
                    var photo = new ProductPhoto
                    {
                        ProductVariantId = addedProductVariant.Id,
                        PhotoUrl = photoUrls[i],
                        AltText = request.ProductVariant.Photos[i].FileName,
                        DisplayOrder = i + 1,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _productPhotoRepository.Insert(photo);
                }

                await _productPhotoRepository.Save();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product variant");
            return false;
        }
    }
}
