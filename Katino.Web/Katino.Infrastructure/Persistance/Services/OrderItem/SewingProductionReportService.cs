using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Repositories.SewingHistoryRepository;
using Katino.Domain.Services.OrderItemN.SewingProductionReportService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderItemN;

public class SewingProductionReportService : ISewingProductionReportService
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISewingHistoryRepository _sewingHistoryRepository;
    private readonly IUpdateProductVariantService _updateProductVariantService;
    private readonly ILogger _logger;

    public SewingProductionReportService(
        IProductVariantRepository productVariantRepository,
        IOrderRepository orderRepository,
        ISewingHistoryRepository sewingHistoryRepository,
        IUpdateProductVariantService updateProductVariantService,
        ILoggerFactory loggerFactory)
    {
        _productVariantRepository = productVariantRepository;
        _orderRepository = orderRepository;
        _sewingHistoryRepository = sewingHistoryRepository;
        _updateProductVariantService = updateProductVariantService;
        _logger = loggerFactory?.CreateLogger(nameof(SewingProductionReportService));
    }

    public async Task ApplySewedAsync(SewedReport report, Guid submittedBy)
    {
        _logger.LogInformation($"Applying sewed report, sewed quantity: {report.ActualSewedQuantity}, product variant id: {report.ProductVariantId}");

        if (report.ActualSewedQuantity <= 0)
        {
            return;
        }

        if (report.OrderItemId is not null)
        {
            await ApplyCustomSewedAsync(report.ProductVariantId, report.OrderItemId.Value, report.ActualSewedQuantity);
        }
        else
        {
            await ApplyRegularSewedAsync(report.ProductVariantId, report.ActualSewedQuantity);
        }

        await _sewingHistoryRepository.Insert(new SewingHistory
        {
            ProductVariantId = report.ProductVariantId,
            SewedBy = submittedBy,
            SewedQuantity = report.ActualSewedQuantity,
            IsCustomTailoring = report.OrderItemId is not null,
            SewedDate = DateTime.UtcNow
        });

        await _sewingHistoryRepository.Save();
    }

    private async Task ApplyRegularSewedAsync(Guid productVariantId, int actualSewedQuantity)
    {
        _logger.LogInformation($"Applying regular sewed item, quantity: {actualSewedQuantity}");

        var pv = await _productVariantRepository.Get(productVariantId);
        var newAvailable = pv.QuantityInStock + actualSewedQuantity;
        await _updateProductVariantService.HandleProductVariantQuantityChange(productVariantId, newAvailable);
    }

    private async Task ApplyCustomSewedAsync(Guid productVariantId, Guid orderItemId, int actualSewedQuantity)
    {
        _logger.LogInformation($"Applying custom sewed item: {orderItemId}");

        var orderItem = await _orderRepository.GetOrderItemWithOrderAsync(orderItemId);

        if (!orderItem.IsCustomTailoring)
        {
            throw new InvalidOperationException($"OrderItemId {orderItemId} is not custom");
        }

        if (orderItem.ProductVariantId != productVariantId)
        {
            throw new InvalidOperationException($"OrderItemId {orderItemId} doesn't belong to ProductVariantId {productVariantId}.");
        }

        if (orderItem.OrderItemStatus == OrderItemStatus.Ready)
        {
            throw new InvalidOperationException($"OrderItem {orderItemId} is already ready.");
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

        await _orderRepository.Save();
    }
}
