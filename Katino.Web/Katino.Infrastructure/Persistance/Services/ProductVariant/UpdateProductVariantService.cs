using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductPhotoRepository;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.AzureStorage;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Katino.Domain.Services.ProductVariantRedistributionN.ProductVariantRedistributionRecorder;
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
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly IProductVariantRedistributionRecorder _redistributionRecorder;
    private readonly IProductVariantRedistributionHistoryRepository _redistributionHistoryRepository;
    private readonly ILogger _logger;

    public UpdateProductVariantService(
        IProductVariantRepository productVariantRepository,
        IOrderRepository orderRepository,
        IProductVariantMeasurementRepository productVariantMeasurementRepository,
        IProductPhotoRepository productPhotoRepository,
        IAzureStorageService azureStorageService,
        IKatinoDbContext katinoDbContext,
        IProductVariantRedistributionRecorder redistributionRecorder,
        IProductVariantRedistributionHistoryRepository redistributionHistoryRepository,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _orderRepository = orderRepository;
        _productVariantMeasurementRepository = productVariantMeasurementRepository;
        _productPhotoRepository = productPhotoRepository;
        _azureStorageService = azureStorageService;
        _katinoDbContext = katinoDbContext;
        _redistributionRecorder = redistributionRecorder;
        _redistributionHistoryRepository = redistributionHistoryRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateProductVariantService));
    }

    public async Task<bool> UpdateProductVariantAsync(ProductVariant productVariant, IFormFileCollection newPhotos, List<Guid> photoIdsToDelete, List<Guid> sewerIds)
    {
        try
        {
            var productVariantFromDb = await _productVariantRepository.GetWithMeasurementsAndSewers(productVariant.Id);
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
            productVariantFromDb.SewingQueueVisibility = productVariant.SewingQueueVisibility;

            await using var transaction = await _katinoDbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var measurement in productVariantFromDb.Measurements)
                {
                    _productVariantMeasurementRepository.Delete(measurement);
                }

                await _productVariantMeasurementRepository.Save();

                productVariantFromDb.Measurements.Clear();

                productVariantFromDb.Measurements.AddRange(productVariant.Measurements);

                foreach (var sewer in productVariantFromDb.Sewers)
                {
                    _productVariantRepository.DeleteSewer(sewer);
                }

                await _productVariantRepository.Save();

                productVariantFromDb.Sewers.Clear();

                if (productVariant.SewingQueueVisibility == SewingQueueVisibility.Specific)
                {
                    productVariantFromDb.Sewers.AddRange(sewerIds.Select(id => new ProductVariantSewer
                    {
                        ProductVariantId = productVariantFromDb.Id,
                        SewerId = id
                    }));
                }

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

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during updating product variant");
                await transaction.RollbackAsync();
                throw;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating product variant: {productVariant.Id}");
            return false;
        }
    }

    public async Task HandleProductVariantQuantityChangeSplit(
        Guid productVariantId,
        int originalQuantityInStock,
        int realDelta,
        int phantomDelta,
        Guid? orderIdToSkipFromProcessing,
        ProductVariantQuantityChangeReason reason,
        string sourceOrderTtnSnapshot = null)
    {
        var currentBase = originalQuantityInStock;

        if (realDelta > 0)
        {
            await HandleProductVariantQuantityChange(
                productVariantId,
                currentBase + realDelta,
                orderIdToSkipFromProcessing,
                reason,
                sourceOrderTtnSnapshot,
                isPendingPhysicalArrival: false);

            var productVariantAfterRealPass = await _productVariantRepository.GetAsNoTracking(productVariantId);
            currentBase = productVariantAfterRealPass.QuantityInStock;
        }

        if (phantomDelta > 0)
        {
            await HandleProductVariantQuantityChange(
                productVariantId,
                currentBase + phantomDelta,
                orderIdToSkipFromProcessing,
                reason,
                sourceOrderTtnSnapshot,
                isPendingPhysicalArrival: true);
        }
    }

    public async Task HandleProductVariantQuantityChange(
        Guid productVariantId,
        int newQuantity,
        Guid? orderIdToSkipFromProcessing = null,
        ProductVariantQuantityChangeReason reason = ProductVariantQuantityChangeReason.ManualEdit,
        string sourceOrderTtnSnapshot = null,
        bool isPendingPhysicalArrival = false)
    {
        var ordersToCheck = await _orderRepository.GetActiveOrdersWithSpecificProductVariantAsync(productVariantId);
        int currentProductVariantQuantity = newQuantity;
        List<ProductVariantRedistributionLine> redistributionLines = [];
        foreach (var order in ordersToCheck)
        {
            if (currentProductVariantQuantity <= 0)
            {
                break;
            }

            if (orderIdToSkipFromProcessing != null && order.Id == orderIdToSkipFromProcessing)
            {
                continue;
            }

            // An order can have several items of the same product variant (e.g. one already covered by
            // real stock, another still needing coverage) - all of them must be considered, not just the
            // first match, otherwise a genuinely needy sibling item can be silently skipped.
            var requiredOrderItems = order.OrderItems
                .Where(i => i.ProductVariantId == productVariantId && !i.IsCustomTailoring && i.OrderItemStatus != OrderItemStatus.Ready)
                .ToList();

            if (requiredOrderItems.Count == 0)
            {
                continue;
            }

            foreach (var requiredOrderItem in requiredOrderItems)
            {
                if (currentProductVariantQuantity <= 0)
                {
                    break;
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

                var pendingRemaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(requiredOrderItem.Id);
                PendingReturnInvariantHelper.CheckOrderItemInvariant(requiredOrderItem, pendingRemaining, _logger);

                var quantityAssignedToOrder = previousQuantityToProduce - newOderItemQuantityToProduce;
                redistributionLines.Add(new ProductVariantRedistributionLine
                {
                    TargetOrderId = order.Id,
                    TargetOrderItemId = requiredOrderItem.Id,
                    Quantity = quantityAssignedToOrder
                });

                var newQuantityInStock = currentProductVariantQuantity < previousQuantityToProduce
                    ? 0
                    : currentProductVariantQuantity - previousQuantityToProduce;

                currentProductVariantQuantity = newQuantityInStock;
            }

            var newOrderStatus = order.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
                ? OrderStatus.InProgress
                : OrderStatus.ReadyToShip;

            var orderStatusChanged = order.OrderStatus != newOrderStatus;
            OrderStatusHelper.SetOrderStatus(order, newOrderStatus, false);
            if (orderStatusChanged)
            {
                order.UpdatedAt = DateTimeOffset.UtcNow;
                order.UpdateReasonDetails = $"Status changed due to product variant quantity change (id: '{productVariantId}', new quantity: {newQuantity})";
            }

            await _orderRepository.Save();
        }

        var productVariantFromDb = await _productVariantRepository.Get(productVariantId);
        productVariantFromDb.QuantityInStock = currentProductVariantQuantity;
        if (productVariantFromDb.Status != ProductStatus.Discontinued)
        {
            productVariantFromDb.Status = currentProductVariantQuantity > 0 ? ProductStatus.InStock : ProductStatus.OnOrder;
        }

        await _productVariantRepository.Update(productVariantFromDb);
        await _productVariantRepository.Save();

        if (currentProductVariantQuantity > 0)
        {
            redistributionLines.Add(new ProductVariantRedistributionLine
            {
                TargetOrderId = null,
                TargetOrderItemId = null,
                Quantity = currentProductVariantQuantity
            });
        }

        await _redistributionRecorder.RecordAsync(new ProductVariantRedistributionEvent
        {
            ProductVariantId = productVariantId,
            Reason = reason,
            // For OrderDeleted the source order row is already gone from the DB by this point
            // (DeleteOrderService removes it before redistributing), so the FK would dangle.
            // The TTN snapshot is what keeps that order identifiable for display.
            SourceOrderId = reason == ProductVariantQuantityChangeReason.OrderDeleted ? null : orderIdToSkipFromProcessing,
            SourceOrderTtnSnapshot = sourceOrderTtnSnapshot,
            IsPendingPhysicalArrival = isPendingPhysicalArrival,
            Lines = redistributionLines
        });
    }
}
