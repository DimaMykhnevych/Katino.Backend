using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.ProductVariantN;

public class UpdateProductVariantService : IUpdateProductVariantService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IProductVariantMeasurementRepository _productVariantMeasurementRepository;
    private readonly ILogger _logger;

    public UpdateProductVariantService(
        IProductVariantRepository productVariantRepository,
        IProductVariantMeasurementRepository productVariantMeasurementRepository,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _productVariantMeasurementRepository = productVariantMeasurementRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateProductVariantService));
    }

    public async Task<bool> UpdateProductVariantAsync(ProductVariant productVariant)
    {
        // TODO
        // 4. on UI prouct properties update should be disabled on product variant update page,
        // size and color however can be modified

        try
        {
            var productVariantFromDb = await _productVariantRepository.GetWithMeasurements(productVariant.Id);
            productVariantFromDb.Status = productVariant.Status;
            if (productVariantFromDb.AvailableQuantity == 0 && productVariantFromDb.Status != ProductStatus.Discontinued)
            {
                productVariant.Status = ProductStatus.OnOrder;
            }

            productVariantFromDb.SizeId = productVariant.SizeId;
            productVariantFromDb.ColorId = productVariant.ColorId;
            productVariantFromDb.QuantityInStock = productVariant.QuantityInStock;
            productVariantFromDb.QuantityDropSold = productVariant.QuantityDropSold;
            productVariantFromDb.QuantityRegularSold = productVariant.QuantityRegularSold;
            productVariantFromDb.IsDrop = productVariant.IsDrop;

            foreach (var measurement in productVariantFromDb.Measurements)
            {
                _productVariantMeasurementRepository.Delete(measurement);
            }

            await _productVariantMeasurementRepository.Save();

            productVariantFromDb.Measurements.Clear();

            productVariantFromDb.Measurements.AddRange(productVariant.Measurements);

            await _productVariantRepository.Update(productVariantFromDb);
            await _productVariantRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating product variant: {productVariant.Id}");
            return false;
        }
    }
}
