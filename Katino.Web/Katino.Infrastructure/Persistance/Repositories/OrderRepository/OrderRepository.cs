using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.OrderRepository;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<List<Order>> GetActiveOrdersWithSpecificProductVariantAsync(Guid productVariantId)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderReadinessStatus == OrderReadinessStatus.InProgress && o.OrderItems.Any(i => i.ProductVariantId == productVariantId))
            .OrderBy(o => o.SendUntilDate)
            .ToListAsync();
    }

    public async Task<Order> GetExistingOrderForUpdate(Guid orderId)
    {
        return await context.Orders
            .Include(o => o.OrderNpOptionsSeats)
                .ThenInclude(o => o.NpOptionsSeat)
            .Include(o => o.SenderNpWarehouse)
            .Include(o => o.RecipientNpWarehouse)
            .Include(o => o.SenderNpCity)
            .Include(o => o.RecipientNpCity)
            .Include(o => o.SenderContactPerson)
            .Include(o => o.OrderItems)
            .Include(o => o.OrderRecipient)
                .ThenInclude(o => o.NpContactPerson)
            .Include(o => o.AddressInfo)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order> GetExistingOrderForDelete(Guid orderId)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.AddressInfo)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<Order> GetOrderWithInfoForInternetDocCreation(Guid orderId)
    {
        return await context.Orders
            .Include(o => o.OrderNpOptionsSeats)
                .ThenInclude(o => o.NpOptionsSeat)
            .Include(o => o.SenderNpWarehouse)
            .Include(o => o.RecipientNpWarehouse)
            .Include(o => o.SenderNpCity)
            .Include(o => o.RecipientNpCity)
            .Include(o => o.SenderContactPerson)
            .Include(o => o.OrderRecipient)
                .ThenInclude(o => o.NpContactPerson)
            .Include(o => o.AddressInfo)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetOrdersForNpStatusUpdateAsync(OrderInternetDocStatus[] statusesToExclude)
    {
        return await context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => !statusesToExclude.Contains(o.OrderInternetDocStatus) && !string.IsNullOrEmpty(o.InternetDocumentIntDocNumber))
            .ToListAsync();
    }

    public Task UpdateInternetDocStatusAsync(Guid orderId, OrderInternetDocStatus status)
    {
        return context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.OrderInternetDocStatus, status));
    }

    public async Task<Order> GetOrderWithOrderItemsAsync(Guid orderId)
    {
        return await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
