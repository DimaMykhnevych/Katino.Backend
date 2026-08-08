using AutoMapper;
using Katino.Application.DTOs.ProductVariant;
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

            bool variantAlreadyExists = await _productVariantRepository.ExistsActiveBySizeAndColorAsync(
                productVariant.ProductId, productVariant.SizeId, productVariant.ColorId);
            if (variantAlreadyExists)
            {
                _logger.LogWarning(
                    "An active product variant with ProductId = {productId}, SizeId = {sizeId}, ColorId = {colorId} already exists",
                    productVariant.ProductId, productVariant.SizeId, productVariant.ColorId);
                return false;
            }

            if (productVariant.QuantityInStock <= 0 && productVariant.Status != ProductStatus.Discontinued)
            {
                productVariant.Status = ProductStatus.OnOrder;
            }

            if (request.ProductVariant.SewingQueueVisibility == SewingQueueVisibilityDto.Specific)
            {
                productVariant.Sewers = request.ProductVariant.SewerIds
                    .Select(sewerId => new ProductVariantSewer { SewerId = sewerId })
                    .ToList();
            }

            var addedProductVariant = await _productVariantRepository.Insert(productVariant);

            var photosCount = request.ProductVariant.ExistingPhotoUrls.Count;
            if (photosCount > 0)
            {
                for (int i = 0; i < request.ProductVariant.ExistingPhotoUrls.Count; i++)
                {
                    var newUrl = await _azureStorageService.CopyPhotoAsync(
                        request.ProductVariant.ExistingPhotoUrls[i],
                        addedProductVariant.Id);

                    var copiedPhoto = new ProductPhoto
                    {
                        ProductVariantId = addedProductVariant.Id,
                        PhotoUrl = newUrl,
                        AltText = "Product photo",
                        DisplayOrder = i + 1,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _productPhotoRepository.Insert(copiedPhoto);
                }
            }

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
                        DisplayOrder = photosCount + i + 1,
                        UploadedAt = DateTime.UtcNow
                    };

                    await _productPhotoRepository.Insert(photo);
                }
            }

            await _productPhotoRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product variant");
            return false;
        }
    }
}
