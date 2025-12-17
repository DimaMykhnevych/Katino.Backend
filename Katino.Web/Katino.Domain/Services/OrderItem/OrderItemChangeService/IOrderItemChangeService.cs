using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Services.OrderItemN.OrderItemChangeService;

public interface IOrderItemChangeService
{
    Task HandleDeletedOrderItems(SaleType saleType, List<OrderItem> deletedItems);
    Task HandleAddedOrderItems(SaleType saleType, List<OrderItem> orderItems);
    Task ProcessCurrentOrderItemsStatuses(List<OrderItem> orderItems);
}
