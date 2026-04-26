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

    public async Task<List<OrderItem>> GetOrderItemsForSewingAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        return await context.OrderItems
            .AsNoTracking()
            .AsSplitQuery()
            .Where(oi =>
                oi.Order.OrderStatus == OrderStatus.InProgress &&
                oi.OrderItemStatus == OrderItemStatus.ForSewing &&
                (sewerId == null ||
                 oi.ProductVariant.SewingQueueVisibility == SewingQueueVisibility.AllSewers ||
                 (oi.ProductVariant.SewingQueueVisibility == SewingQueueVisibility.Specific &&
                  oi.ProductVariant.Sewers.Any(s => s.SewerId == sewerId))))
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Product)
                    .ThenInclude(p => p.Category)
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Color)
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Size)
            .Include(oi => oi.ProductVariant)
                .ThenInclude(pv => pv.Photos)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<DateTime, List<OrderItem>>> GetOrderItemsForSewingGroupedByDateAsync(Guid? sewerId = null, CancellationToken ct = default)
    {
        var items = await context.OrderItems
            .AsNoTracking()
            .AsSplitQuery()
            .Where(oi =>
                oi.Order.OrderStatus == OrderStatus.InProgress &&
                oi.OrderItemStatus == OrderItemStatus.ForSewing &&
                (sewerId == null ||
                 oi.ProductVariant.SewingQueueVisibility == SewingQueueVisibility.AllSewers ||
                 (oi.ProductVariant.SewingQueueVisibility == SewingQueueVisibility.Specific &&
                  oi.ProductVariant.Sewers.Any(s => s.SewerId == sewerId))))
            .Include(oi => oi.Order)
            .Include(oi => oi.ProductVariant).ThenInclude(pv => pv.Product).ThenInclude(p => p.Category)
            .Include(oi => oi.ProductVariant).ThenInclude(pv => pv.Color)
            .Include(oi => oi.ProductVariant).ThenInclude(pv => pv.Size)
            .Include(oi => oi.ProductVariant).ThenInclude(pv => pv.Photos)
            .ToListAsync(ct);

        return items
            .GroupBy(oi => oi.Order.SendUntilDate.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
