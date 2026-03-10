using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Repositories.OrderRepository;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order> GetOrderWithInfoForInternetDocCreation(Guid orderId);
    Task<Order> GetExistingOrderForUpdate(Guid orderId);
    Task<Order> GetExistingOrderForDelete(Guid orderId);
    Task<List<Order>> GetActiveOrdersWithSpecificProductVariantAsync(Guid productVariantId);
    Task<List<Order>> GetOrdersForNpStatusUpdateAsync(OrderInternetDocStatus[] statusesToExclude);
    Task UpdateInternetDocStatusAsync(Guid orderId,
        OrderInternetDocStatus status,
        OrderStatus orderStatus,
        bool shouldUpdateOrderStatus);
    Task<Order> GetOrderWithOrderItemsAsync(Guid orderId);
    Task<OrderItem> GetOrderItemWithOrderAsync(Guid orderItemId);
    Task<List<string>> GetOrderRefsWithStatusAndInternetDocCreatedAsync(OrderStatus status);
}
