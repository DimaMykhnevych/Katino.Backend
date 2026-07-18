using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Services.OrderItemN.OrderItemChangeService;

public interface IOrderItemChangeService
{
    // phantomQuantitiesFreed accumulates, per ProductVariantId, how much of the newly-freed quantity in
    // productQuantitiesAfterProcessing is still physically in transit (an open pending return being taken
    // over from the deleted/edited order item) rather than genuinely available stock. Callers use it to
    // redistribute the real and phantom parts separately via IUpdateProductVariantService.HandleProductVariantQuantityChangeSplit.
    Task HandleDeletedOrderItems(
        SaleType saleType,
        List<OrderItem> deletedItems,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        Dictionary<Guid, int> phantomQuantitiesFreed,
        List<ProductVariant> productVariants,
        bool deleteOrderItems = true);
    Task HandleAddedOrderItems(
        SaleType saleType,
        List<OrderItem> orderItems,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        List<ProductVariant> productVariants);
    Task HandleUpdatedOrderItems(
        SaleType saleType,
        List<OrderItem> itemsToUpdate,
        List<OrderItem> existingOrderItemsFromDb,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        Dictionary<Guid, int> phantomQuantitiesFreed,
        List<ProductVariant> productVariants);
    Task HandleOrderItemsReturn(
        Order order,
        Dictionary<Guid, int> currentProductQuantities,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
        Dictionary<Guid, int> phantomQuantitiesFreed);
    void ProcessNewOrderItemsStatuses(List<OrderItem> orderItems, List<ProductVariant> relatedProductVariants);
}
