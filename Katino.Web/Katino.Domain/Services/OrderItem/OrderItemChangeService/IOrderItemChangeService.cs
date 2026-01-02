using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Services.OrderItemN.OrderItemChangeService;

public interface IOrderItemChangeService
{
    Task HandleDeletedOrderItems(
        SaleType saleType,
        List<OrderItem> deletedItems,
        Dictionary<Guid, int> productQuantitiesAfterProcessing,
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
        List<ProductVariant> productVariants);
    Task HandleOrderItemsReturn(
        Order order,
        Dictionary<Guid, int> currentProductQuantities,
        Dictionary<Guid, int> productQuantitiesAfterProcessing);
    void ProcessNewOrderItemsStatuses(List<OrderItem> orderItems, List<ProductVariant> relatedProductVariants);
}
