using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.OrderItemRepository;

public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<List<OrderItem>> GetOrderItemsForSewingAsync(CancellationToken ct = default)
    {
        return await context.OrderItems
            .AsNoTracking()
            .AsSplitQuery()
            .Where(oi =>
                oi.Order.OrderStatus == OrderStatus.InProgress &&
                oi.OrderItemStatus == OrderItemStatus.ForSewing)
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Category)
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Color)
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Size)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<DateTime, List<OrderItem>>> GetOrderItemsForSewingGroupedByDateAsync(CancellationToken ct = default)
    {
        var items = await context.OrderItems
            .AsNoTracking()
            .AsSplitQuery()
            .Where(oi =>
                oi.Order.OrderStatus == OrderStatus.InProgress &&
                oi.OrderItemStatus == OrderItemStatus.ForSewing)
            .Include(oi => oi.Order)
            .Include(oi => oi.ProductVariant).ThenInclude(pv => pv.Product).ThenInclude(p => p.Category)
            .Include(oi => oi.ProductVariant).ThenInclude(pv => pv.Color)
            .Include(oi => oi.ProductVariant).ThenInclude(pv => pv.Size)
            .ToListAsync(ct);

        return items
            .GroupBy(oi => oi.Order.SendUntilDate.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
