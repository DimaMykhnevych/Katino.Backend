using Katino.Domain.Constants;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Helpers;
using Katino.Domain.Models;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Domain.Repositories.FinanceEntryRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Katino.Domain.Services.OrderN.OrderDeliveryHandler;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.AddOrderService;
using Katino.Domain.Services.OrderN.OrderPricingService;
using Katino.Domain.Services.OrderN.UrgentOrderRedistributionService;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class AddOrderService : IAddOrderService
{
    private readonly IAddNpCityService _addNpCityService;
    private readonly IAddNpContactPersonService _addNpContactPersonService;
    private readonly IOrderDeliveryHandlerFactory _deliveryHandlerFactory;
    private readonly IOrderItemChangeService _orderItemChangeService;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderTagRepository _orderTagRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IFinanceEntryRepository _financeEntryRepository;
    private readonly IFinanceCategoryRepository _financeCategoryRepository;
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly IUrgentOrderRedistributionService _urgentOrderRedistributionService;
    private readonly IOrderPricingService _orderPricingService;
    private readonly ILogger _logger;

    public AddOrderService(
        IAddNpCityService addNpCityService,
        IAddNpContactPersonService addNpContactPersonService,
        IOrderDeliveryHandlerFactory deliveryHandlerFactory,
        IOrderItemChangeService orderItemChangeService,
        IOrderRepository orderRepository,
        IOrderTagRepository orderTagRepository,
        IProductVariantRepository productVariantRepository,
        IFinanceEntryRepository financeEntryRepository,
        IFinanceCategoryRepository financeCategoryRepository,
        IKatinoDbContext katinoDbContext,
        IUrgentOrderRedistributionService urgentOrderRedistributionService,
        IOrderPricingService orderPricingService,
        ILoggerFactory loggerFactory)
    {
        _addNpCityService = addNpCityService;
        _addNpContactPersonService = addNpContactPersonService;
        _deliveryHandlerFactory = deliveryHandlerFactory;
        _orderRepository = orderRepository;
        _orderTagRepository = orderTagRepository;
        _katinoDbContext = katinoDbContext;
        _orderItemChangeService = orderItemChangeService;
        _productVariantRepository = productVariantRepository;
        _financeEntryRepository = financeEntryRepository;
        _financeCategoryRepository = financeCategoryRepository;
        _urgentOrderRedistributionService = urgentOrderRedistributionService;
        _orderPricingService = orderPricingService;
        _logger = loggerFactory?.CreateLogger(nameof(AddOrderService));
    }

    public async Task<OrderCreationResult> AddAsync(Order order, List<string> customTags, bool recalculateCost = false)
    {
        _logger.LogInformation($"Adding order, order items count: {order.OrderItems.Count}");

        try
        {
            _logger.LogTrace($"Upserting sender city {order.SenderNpCity.Present}");
            var senderNpCityId = await _addNpCityService.UpsertNpCityAsync(order.SenderNpCity);

            Guid? recipientNpCityId = null;
            if (order.DeliveryType == DeliveryType.WarehouseOrPost)
            {
                _logger.LogTrace($"Upserting recipient city {order.RecipientNpCity.Present}");
                recipientNpCityId = await _addNpCityService.UpsertNpCityAsync(order.RecipientNpCity);
            }

            _logger.LogTrace($"Upserting sender contact person {order.SenderContactPerson.Phones}");
            var senderContactPersonId = await _addNpContactPersonService.UpsertNpContactPersonAsync(order.SenderContactPerson);

            var deliveryHandler = _deliveryHandlerFactory.Create(order.DeliveryType);

            _logger.LogTrace("Resolving order recipient");
            var orderRecipientId = await deliveryHandler.ResolveOrderRecipientAsync(order);

            _logger.LogTrace("Resolving NP options seats");
            var npOptionSeats = await deliveryHandler.ResolveNpOptionSeatsAsync(order.OrderNpOptionsSeats);

            // 1. Resolve order cost
            decimal finalCost;
            double? finalAfterpayment = order.AfterpaymentOnGoodsCost;

            if (recalculateCost)
            {
                _logger.LogTrace("Calculating order cost with discounts");
                var pricingResult = await _orderPricingService.CalculateAsync(order.OrderItems, order.SaleType);
                _logger.LogDebug("Order pricing: base={Base}, discount={Discount}, final={Final}",
                    pricingResult.BaseTotal, pricingResult.TotalDiscount, pricingResult.FinalTotal);
                finalCost = pricingResult.FinalTotal;
                if (order.AfterpaymentOnGoodsCost.HasValue)
                    finalAfterpayment = (double)(pricingResult.FinalTotal - 200m);
            }
            else
            {
                finalCost = (decimal)order.Cost;
            }

            // 2. Process orderItems (set quantity to produce + order item statuses)
            _logger.LogTrace("Processing order items statuses");

            List<ProductVariant> productVariantsRelatedToCurrentOrder = [];
            var newProductVariantIds = order.OrderItems.Select(i => i.ProductVariantId).ToList();
            foreach (var productVariantId in newProductVariantIds)
            {
                var productVariant = await _productVariantRepository.GetAsNoTracking(productVariantId);
                productVariantsRelatedToCurrentOrder.Add(productVariant);
            }

            // ProcessNewOrderItemsStatuses must be called before HandleAddedOrderItems to avoid
            // incorrect statuses due to quantity changes during HandleAddedOrderItems
            _orderItemChangeService.ProcessNewOrderItemsStatuses(order.OrderItems, productVariantsRelatedToCurrentOrder);

            var utcNow = DateTimeOffset.UtcNow;
            Order orderToAdd = new()
            {
                SenderNpWarehouseId = order.SenderNpWarehouseId,
                RecipientNpWarehouseId = order.RecipientNpWarehouseId,
                SenderNpCityId = senderNpCityId,
                RecipientNpCityId = recipientNpCityId,
                SenderContactPersonId = senderContactPersonId,
                OrderRecipientId = orderRecipientId,
                PayerType = order.PayerType,
                PaymentMethod = order.PaymentMethod,
                SaleType = order.SaleType,
                CreationDateTime = order.CreationDateTime != default ? order.CreationDateTime : utcNow,
                CreationDateTimeSystem = utcNow,
                SendUntilDate = order.SendUntilDate,
                Weight = order.Weight,
                DeliveryType = order.DeliveryType,
                SeatsAmount = order.SeatsAmount,
                Description = order.Description,
                Cost = (double)finalCost,
                AfterpaymentOnGoodsCost = finalAfterpayment,
                OrderItems = order.OrderItems,
                OrderNpOptionsSeats = npOptionSeats,
                AddressInfo = order.AddressInfo,
                OrderInternetDocStatus = OrderInternetDocStatus.NotProcessed,
                OrderStatus = OrderStatus.None,
                Comment = order.Comment,
                GeneralOrderInfo = order.GeneralOrderInfo,
                UpdatedAt = utcNow,
                UpdateReasonDetails = "Order created"
            };

            // 2. Calculate Order status
            var newOrderStatus = order.OrderItems.Any(i => i.OrderItemStatus == OrderItemStatus.ForSewing)
                ? OrderStatus.InProgress
                : OrderStatus.ReadyToShip;
            OrderStatusHelper.SetOrderStatus(orderToAdd, newOrderStatus, false);

            // 3. Save order
            _logger.LogTrace("Saving order in db");
            await using var transaction = await _katinoDbContext.Database.BeginTransactionAsync();

            var insertedOrder = await _orderRepository.Insert(orderToAdd);
            await InsertFinanceEntry(orderToAdd, insertedOrder.Id).ConfigureAwait(false);
            try
            {
                // 4. Attach system tags
                if (order.DeliveryType == DeliveryType.NotNovaPost)
                {
                    _logger.LogTrace("Attaching NotNpOrder tag to order");
                    var tag = await _orderTagRepository.GetOrCreateByTypeAsync(OrderTagType.NotNpOrder, canBeDeleted: false);
                    await _orderTagRepository.AttachTagToOrderAsync(insertedOrder.Id, tag.Id);
                }

                // 4.1 Attach custom tags
                foreach (var tagValue in customTags)
                {
                    _logger.LogTrace("Attaching custom tag '{TagValue}' to order", tagValue);
                    var customTag = await _orderTagRepository.GetOrCreateCustomTagAsync(tagValue);
                    await _orderTagRepository.AttachTagToOrderAsync(insertedOrder.Id, customTag.Id);
                }

                // 5. Update product variant quantities
                _logger.LogTrace("Updating order product variant quantities (handling added order items)");
                await _orderItemChangeService.HandleAddedOrderItems(order.SaleType, order.OrderItems, [], productVariantsRelatedToCurrentOrder);

                await _productVariantRepository.Save();

                // 6. Redistribute stock from less urgent orders if the new order has items still needing production
                if (orderToAdd.OrderStatus == OrderStatus.InProgress)
                {
                    _logger.LogTrace("Order has ForSewing items, attempting redistribution from less urgent orders");
                    await _urgentOrderRedistributionService.RedistributeForUrgentOrderAsync(orderToAdd);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during saving order and updating product variant quantities");
                await transaction.RollbackAsync();
                throw;
            }

            // 5. Handle internet document (NP: create TTN; non-NP: no-op)
            var npInternetDocCreated = await deliveryHandler.HandleInternetDocumentOnAddAsync(insertedOrder);

            return new() { OrderAddedSuccessfully = true, NpInternetDocCreatedSuccessfully = npInternetDocCreated, CalculatedCost = finalCost };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while adding order");
            return new();
        }
    }

    private async Task InsertFinanceEntry(Order orderToAdd, Guid insertedOrderId)
    {
        var revenueCategoryId = await GetRevenueCategoryIdAsync();

        var revenueEntry = new FinanceEntry
        {
            EntryDate = DateTimeHelper.ToKyivDateTime(orderToAdd.CreationDateTime).Date,
            Amount = Convert.ToDecimal(orderToAdd.Cost),
            Comment = null,
            SourceType = FinanceEntrySourceType.Order,
            Reason = FinanceEntryReason.None,
            SaleType = orderToAdd.SaleType,
            IsLocked = true,
            InternetDocumentIntDocNumber = null,
            CategoryId = revenueCategoryId,
            OrderId = insertedOrderId,
            CreatedBy = null,
            ReversedEntryId = null,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _financeEntryRepository.Insert(revenueEntry);
    }

    private async Task<Guid> GetRevenueCategoryIdAsync()
    {
        var existing = await _financeCategoryRepository
            .GetByTypeAndNameAsync(FinanceCategoryType.Income, FinanceCategoryNames.Revenue);
        return existing.Id;
    }
}
