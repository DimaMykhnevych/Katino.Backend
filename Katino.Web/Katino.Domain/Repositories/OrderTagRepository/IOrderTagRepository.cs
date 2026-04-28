using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Repositories.OrderTagRepository;

public interface IOrderTagRepository : IRepository<OrderTag>
{
    Task<OrderTag> GetOrCreateByTypeAsync(OrderTagType type, bool canBeDeleted);
    Task<OrderTag> GetOrCreateCustomTagAsync(string value);
    Task AttachTagToOrderAsync(Guid orderId, Guid tagId);
    Task DetachTagFromOrderAsync(Guid orderId, Guid tagId);
    Task DetachAllCustomTagsFromOrderAsync(Guid orderId);
    Task<IEnumerable<OrderTag>> GetFilteredAsync(string search, bool? customOnly);
}
