using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Repositories.SewingHistoryRepository;
using Katino.Domain.Services.OrderItemN.SewingProductionReportService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Katino.Domain.Services.ProductVariantRedistributionN.ProductVariantRedistributionRecorder;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderItemN;

public class SewingProductionReportService : ISewingProductionReportService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISewingHistoryRepository _sewingHistoryRepository;
    private readonly IProductVariantRedistributionHistoryRepository _redistributionHistoryRepository;
    private readonly IOrderTagRepository _orderTagRepository;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly IProductVariantRedistributionRecorder _redistributionRecorder;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;

    public SewingProductionReportService(
        IProductVariantRepository productVariantRepository,
        IOrderRepository orderRepository,
        ISewingHistoryRepository sewingHistoryRepository,
        IProductVariantRedistributionHistoryRepository redistributionHistoryRepository,
        IOrderTagRepository orderTagRepository,
        IUpdateProductVariantService updateProductVariantService,
        IProductVariantRedistributionRecorder redistributionRecorder,
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _orderRepository = orderRepository;
        _sewingHistoryRepository = sewingHistoryRepository;
        _redistributionHistoryRepository = redistributionHistoryRepository;
        _orderTagRepository = orderTagRepository;
        _updateProductVariantService = updateProductVariantService;
        _redistributionRecorder = redistributionRecorder;
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(SewingProductionReportService));
    }

    public async Task ApplySewedAsync(List<SewedReport> report, Guid submittedBy)
    {
        foreach (var reportItem in report)
        {
            _logger.LogInformation($"Applying sewed report, sewed quantity: {reportItem.ActualSewedQuantity}, product variant id: {reportItem.ProductVariantId}");

            if (reportItem.ActualSewedQuantity <= 0)
            {
                continue;
            }

            var isCustomTailoring = false;

            if (reportItem.OrderItemId is not null)
            {
                var orderItem = await _orderRepository.GetOrderItemWithOrderAsync(reportItem.OrderItemId.Value);
                isCustomTailoring = orderItem.IsCustomTailoring;

                if (orderItem.IsCustomTailoring)
                {
                    await ApplyCustomSewedAsync(orderItem, reportItem.ProductVariantId, reportItem.ActualSewedQuantity);
                }
                else
                {
                    await ApplyIncomingReturnSewedAsync(orderItem, reportItem.ProductVariantId, reportItem.ActualSewedQuantity);
                }
            }
            else
            {
                await ApplyRegularSewedAsync(reportItem.ProductVariantId, reportItem.ActualSewedQuantity);
            }

            await _sewingHistoryRepository.Insert(new SewingHistory
            {
                ProductVariantId = reportItem.ProductVariantId,
                SewedBy = submittedBy,
                SewedQuantity = reportItem.ActualSewedQuantity,
                IsCustomTailoring = isCustomTailoring,
                SewedDate = DateTimeOffset.UtcNow
            });
        }

        await _sewingHistoryRepository.Save();
    }

    private async Task ApplyRegularSewedAsync(Guid productVariantId, int actualSewedQuantity)
    {
        _logger.LogInformation($"Applying regular sewed item, quantity: {actualSewedQuantity}");

        var pv = await _productVariantRepository.Get(productVariantId);
        var newAvailable = pv.QuantityInStock + actualSewedQuantity;
        await _updateProductVariantService.HandleProductVariantQuantityChange(
            productVariantId,
            newAvailable,
            reason: ProductVariantQuantityChangeReason.Sewing);
    }

    private async Task ApplyCustomSewedAsync(OrderItem orderItem, Guid productVariantId, int actualSewedQuantity)
    {
        _logger.LogInformation($"Applying custom sewed item: {orderItem.Id}");

        if (orderItem.ProductVariantId != productVariantId)
        {
            throw new InvalidOperationException($"OrderItemId {orderItem.Id} doesn't belong to ProductVariantId {productVariantId}.");
        }

        if (orderItem.OrderItemStatus == OrderItemStatus.Ready)
        {
            throw new InvalidOperationException($"OrderItem {orderItem.Id} is already ready.");
        }

        if (actualSewedQuantity > orderItem.QuantityToProduce)
        {
            throw new InvalidOperationException(
                $"Actual sewed quantity is more than required quantity to produce for custom item. Required: {orderItem.QuantityToProduce}, actual: {actualSewedQuantity}");
        }

        orderItem.QuantityToProduce -= actualSewedQuantity;
        orderItem.OrderItemStatus = orderItem.QuantityToProduce > 0
            ? OrderItemStatus.ForSewing
            : OrderItemStatus.Ready;

        var order = orderItem.Order;
        var newOrderStatus = order.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
            ? OrderStatus.InProgress
            : OrderStatus.ReadyToShip;

        OrderStatusHelper.SetOrderStatus(order, newOrderStatus, false);
        if (order.OrderStatus != newOrderStatus)
        {
            order.UpdatedAt = DateTimeOffset.UtcNow;
            order.UpdateReasonDetails = $"Status changed due to product variant quantity change (id: '{productVariantId}', new quantity: {actualSewedQuantity})";
        }

        await _orderRepository.Save();

        await _redistributionRecorder.RecordAsync(new ProductVariantRedistributionEvent
        {
            ProductVariantId = productVariantId,
            Reason = ProductVariantQuantityChangeReason.Sewing,
            Lines = [
                new ProductVariantRedistributionLine
                {
                    TargetOrderId = order.Id,
                    TargetOrderItemId = orderItem.Id,
                    Quantity = actualSewedQuantity
                }
            ]
        });
    }

    // The order item is already Ready/QuantityToProduce=0 (the pending return was virtually assigned to it),
    // so there's nothing to update on the order item itself. Sewing a replacement just means: this order no
    // longer needs the physical return, so the still-in-transit quantity it was holding gets freed up and
    // handed to the next most urgent order, same as HandleProductVariantQuantityChange normally does for
    // freshly produced stock.
    private async Task ApplyIncomingReturnSewedAsync(OrderItem orderItem, Guid productVariantId, int actualSewedQuantity)
    {
        _logger.LogInformation($"Applying incoming-return sewed item: {orderItem.Id}");

        if (orderItem.ProductVariantId != productVariantId)
        {
            throw new InvalidOperationException($"OrderItemId {orderItem.Id} doesn't belong to ProductVariantId {productVariantId}.");
        }

        var remaining = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(orderItem.Id);
        if (remaining <= 0)
        {
            throw new InvalidOperationException($"No pending incoming return found for order item {orderItem.Id}.");
        }

        if (actualSewedQuantity > remaining)
        {
            throw new InvalidOperationException(
                $"Actual sewed quantity is more than the pending incoming return amount. Remaining: {remaining}, actual: {actualSewedQuantity}");
        }

        await using var transaction = await _katinoDbContext.Database.BeginTransactionAsync();
        try
        {
            await _redistributionHistoryRepository.ConsumePendingReturnAsync(orderItem.Id, actualSewedQuantity);

            await _redistributionRecorder.RecordAsync(new ProductVariantRedistributionEvent
            {
                ProductVariantId = productVariantId,
                Reason = ProductVariantQuantityChangeReason.ReturnCoveredBySewing,
                Lines = [
                    new ProductVariantRedistributionLine
                    {
                        TargetOrderId = orderItem.OrderId,
                        TargetOrderItemId = orderItem.Id,
                        Quantity = actualSewedQuantity
                    }
                ]
            });

            // The order no longer needs this pending return (a replacement was sewn instead), but the
            // physical item is still in transit - so the freed-up capacity is handed onward still tagged
            // as pending, not as real stock.
            var pv = await _productVariantRepository.Get(productVariantId);
            var newAvailable = pv.QuantityInStock + actualSewedQuantity;
            await _updateProductVariantService.HandleProductVariantQuantityChange(
                productVariantId,
                newAvailable,
                orderIdToSkipFromProcessing: orderItem.OrderId,
                reason: ProductVariantQuantityChangeReason.ReturnCoveredBySewing,
                sourceOrderTtnSnapshot: orderItem.Order.InternetDocumentIntDocNumber,
                isPendingPhysicalArrival: true);

            var pendingRemainingAfter = await _redistributionHistoryRepository.GetPendingReturnRemainingAsync(orderItem.Id);
            PendingReturnInvariantHelper.CheckOrderItemInvariant(orderItem, pendingRemainingAfter, _logger);

            var hasOtherOpenPendingReturns = await _redistributionHistoryRepository.HasOpenPendingReturnsForOrderAsync(orderItem.OrderId);
            if (!hasOtherOpenPendingReturns)
            {
                var tag = await _orderTagRepository.GetOrCreateByTypeAsync(OrderTagType.PendingIncomingReturn, canBeDeleted: true);
                if (await _orderTagRepository.IsTagAttachedToOrderAsync(orderItem.OrderId, tag.Id))
                {
                    await _orderTagRepository.DetachTagFromOrderAsync(orderItem.OrderId, tag.Id);
                    await _orderTagRepository.Save();
                }
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while applying incoming-return sewed item {orderItem.Id}");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
