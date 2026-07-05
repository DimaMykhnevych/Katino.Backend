using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.OrderItemRepository;

public interface IOrderItemRepository : IRepository<OrderItem>
{
    Task<List<OrderItem>> GetOrderItemsForSewingAsync(Guid? sewerId = null, CancellationToken ct = default);
    Task<Dictionary<DateTime, List<OrderItem>>> GetOrderItemsForSewingGroupedByDateAsync(Guid? sewerId = null, CancellationToken ct = default);
    Task<Dictionary<Guid, OrderItem>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
