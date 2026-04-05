using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Repositories.OrderTagRepository;

public interface IOrderTagRepository : IRepository<OrderTag>
{
    Task<OrderTag> GetOrCreateByTypeAsync(OrderTagType type, bool canBeDeleted);
    Task AttachTagToOrderAsync(Guid orderId, Guid tagId);
}
