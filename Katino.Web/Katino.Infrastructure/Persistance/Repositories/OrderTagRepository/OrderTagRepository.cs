using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public async Task<bool> IsTagAttachedToOrderAsync(Guid orderId, Guid tagId)
    {
        return await context.OrderOrderTags
            .AnyAsync(t => t.OrderId == orderId && t.OrderTagId == tagId);
    }

    public async Task DetachTagFromOrderAsync(Guid orderId, Guid tagId)
    {
        var orderOrderTag = await context.OrderOrderTags
            .Include(t => t.OrderTag)
            .FirstOrDefaultAsync(t => t.OrderId == orderId && t.OrderTagId == tagId);

        if (orderOrderTag == null)
        {
            return;
        }

        if (!orderOrderTag.OrderTag.CanBeDeleted)
        {
            throw new InvalidOperationException($"Tag {tagId} cannot be deleted");
        }

        context.OrderOrderTags.Remove(orderOrderTag);
    }

    public async Task DetachAllCustomTagsFromOrderAsync(Guid orderId)
    {
        var customOrderTags = await context.OrderOrderTags
            .Include(t => t.OrderTag)
            .Where(t => t.OrderId == orderId && t.OrderTag.Type == OrderTagType.Custom)
            .ToListAsync();

        context.OrderOrderTags.RemoveRange(customOrderTags);
    }

    public async Task<OrderTag> GetOrCreateCustomTagAsync(string value)
    {
        var normalizedValue = value.ToLower();
        var existing = await context.OrderTags
            .FirstOrDefaultAsync(t => t.Type == OrderTagType.Custom && t.Value.ToLower() == normalizedValue);

        if (existing != null)
        {
            return existing;
        }

        var newTag = new OrderTag
        {
            Id = Guid.NewGuid(),
            Type = OrderTagType.Custom,
            CanBeDeleted = true,
            Value = value,
            CreatedAt = DateTime.UtcNow
        };

        await context.OrderTags.AddAsync(newTag);

        return newTag;
    }

    public async Task<IEnumerable<OrderTag>> GetFilteredAsync(string search, bool? customOnly)
    {
        var query = context.OrderTags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLower();
            query = query.Where(t => t.Value != null && t.Value.ToLower().Contains(normalizedSearch));
        }

        if (customOnly.HasValue && customOnly.Value)
        {
            query = query.Where(t => t.Type == OrderTagType.Custom);
        }

        query = query.OrderBy(t => t.Type).ThenBy(t => t.Value);

        return await query.ToListAsync();
    }
}
