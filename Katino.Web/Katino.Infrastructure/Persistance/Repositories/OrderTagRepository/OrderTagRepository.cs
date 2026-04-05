using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.OrderTagRepository;

public class OrderTagRepository : Repository<OrderTag>, IOrderTagRepository
{
    public OrderTagRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<OrderTag> GetOrCreateByTypeAsync(OrderTagType type, bool canBeDeleted)
    {
        var existing = await context.OrderTags.FirstOrDefaultAsync(t => t.Type == type);
        if (existing != null)
        {
            return existing;
        }

        var newTag = new OrderTag
        {
            Id = Guid.NewGuid(),
            Type = type,
            CanBeDeleted = canBeDeleted,
            CreatedAt = DateTime.UtcNow
        };

        await context.OrderTags.AddAsync(newTag);

        return newTag;
    }

    public async Task AttachTagToOrderAsync(Guid orderId, Guid tagId)
    {
        await context.OrderOrderTags.AddAsync(new OrderOrderTag
        {
            OrderId = orderId,
            OrderTagId = tagId,
            CreatedAt = DateTime.UtcNow
        });
    }
}
