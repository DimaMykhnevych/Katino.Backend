using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.OrderItemRepository;

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<List<OrderItem>> GetOrderItemsForSewingAsync(CancellationToken ct = default);
}
