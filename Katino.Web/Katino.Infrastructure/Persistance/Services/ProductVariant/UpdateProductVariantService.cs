using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductPhotoRepository;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.AzureStorage;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.ProductVariantN;

public class UpdateProductVariantService : IUpdateProductVariantService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductVariantMeasurementRepository _productVariantMeasurementRepository;
    private readonly IProductPhotoRepository _productPhotoRepository;
    private readonly IAzureStorageService _azureStorageService;
    private readonly ILogger _logger;

    public UpdateProductVariantService(
        IProductVariantRepository productVariantRepository,
        IOrderRepository orderRepository,
        IProductVariantMeasurementRepository productVariantMeasurementRepository,
        IProductPhotoRepository productPhotoRepository,
        IAzureStorageService azureStorageService,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _orderRepository = orderRepository;
        _productVariantMeasurementRepository = productVariantMeasurementRepository;
        _productPhotoRepository = productPhotoRepository;
        _azureStorageService = azureStorageService;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateProductVariantService));
    }

    public async Task<bool> UpdateProductVariantAsync(ProductVariant productVariant, IFormFileCollection newPhotos, List<Guid> photoIdsToDelete)
    {
        try
        {
            var productVariantFromDb = await _productVariantRepository.GetWithMeasurements(productVariant.Id);
            productVariantFromDb.Status = productVariant.Status;
            if (productVariantFromDb.QuantityInStock <= 0 && productVariantFromDb.Status != ProductStatus.Discontinued)
            {
                productVariant.Status = ProductStatus.OnOrder;
            }

            bool quantityInStockChanged = productVariantFromDb.QuantityInStock != productVariant.QuantityInStock;

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

            if (quantityInStockChanged)
            {
                await HandleProductVariantQuantityChange(productVariantFromDb.Id, productVariant.QuantityInStock);
            }

            var updatedProductVariant = await _productVariantRepository.GetWithPhotos(productVariant.Id);

            if (photoIdsToDelete.Any())
            {
                var photosToDelete = updatedProductVariant.Photos.Where(p => photoIdsToDelete.Contains(p.Id)).ToList();

                foreach (var photo in photosToDelete)
                {
                    await _azureStorageService.DeletePhotoAsync(updatedProductVariant.Id, photo.PhotoUrl);
                    _productPhotoRepository.Delete(photo);
                }
            }

            if (newPhotos != null && newPhotos.Count > 0)
            {
                var photoUrls = await _azureStorageService.UploadPhotosAsync(updatedProductVariant.Id, newPhotos);
                var maxDisplayOrder = updatedProductVariant.Photos.Any() ? updatedProductVariant.Photos.Max(p => p.DisplayOrder) : 0;

                for (int i = 0; i < photoUrls.Count; i++)
                {
                    var photo = new ProductPhoto
                    {
                        ProductVariantId = updatedProductVariant.Id,
                        PhotoUrl = photoUrls[i],
                        AltText = newPhotos[i].FileName,
                        DisplayOrder = maxDisplayOrder + i + 1,
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
            _logger.LogError(ex, $"An error occurred while updating product variant: {productVariant.Id}");
            return false;
        }
    }

    public async Task HandleProductVariantQuantityChange(Guid productVariantId, int newQuantity)
    {
        var ordersToCheck = await _orderRepository.GetActiveOrdersWithSpecificProductVariantAsync(productVariantId);
        int currentProductVariantQuantity = newQuantity;
        foreach (var order in ordersToCheck)
        {
            if (currentProductVariantQuantity <= 0)
            {
                break;
            }

            var requiredOrderItem = order.OrderItems.First(i => i.ProductVariantId == productVariantId);
            if (requiredOrderItem.IsCustomTailoring || requiredOrderItem.OrderItemStatus == OrderItemStatus.Ready)
            {
                continue;
            }

            var newOderItemQuantityToProduce = currentProductVariantQuantity < requiredOrderItem.QuantityToProduce
                ? requiredOrderItem.QuantityToProduce - currentProductVariantQuantity
                : 0;

            var newOrderItemStatus = newOderItemQuantityToProduce > 0
                ? OrderItemStatus.ForSewing
                : OrderItemStatus.Ready;

            var previousQuantityToProduce = requiredOrderItem.QuantityToProduce;

            requiredOrderItem.QuantityToProduce = newOderItemQuantityToProduce;
            requiredOrderItem.OrderItemStatus = newOrderItemStatus;

            order.OrderReadinessStatus = order.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
                ? OrderReadinessStatus.InProgress
                : OrderReadinessStatus.ReadyToShip;

            await _orderRepository.Update(order);
            await _orderRepository.Save();

            var newQuantityInStock = currentProductVariantQuantity < previousQuantityToProduce
                ? 0
                : currentProductVariantQuantity - previousQuantityToProduce;

            currentProductVariantQuantity = newQuantityInStock;
        }

        if (ordersToCheck.Any())
        {
            var productVariantFromDb = await _productVariantRepository.Get(productVariantId);
            productVariantFromDb.QuantityInStock = currentProductVariantQuantity;
            productVariantFromDb.Status = currentProductVariantQuantity > 0 ? ProductStatus.InStock : ProductStatus.OnOrder;

            await _productVariantRepository.Update(productVariantFromDb);
            await _productVariantRepository.Save();
        }
    }
}
