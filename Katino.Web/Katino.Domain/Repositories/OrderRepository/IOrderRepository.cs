using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.OrderRepository;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order> GetOrderWithInfoForInternetDocCreation(Guid orderId);
    Task<List<Order>> GetActiveOrdersWithSpecificProductVariantAsync(Guid productVariantId);
}
