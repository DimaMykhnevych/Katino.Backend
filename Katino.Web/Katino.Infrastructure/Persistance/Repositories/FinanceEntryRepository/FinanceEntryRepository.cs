using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceEntryRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.FinanceEntryRepository;

public class FinanceEntryRepository : Repository<FinanceEntry>, IFinanceEntryRepository
{
    public FinanceEntryRepository(KatinoDbContext context) : base(context) { }

    public Task<FinanceEntry?> GetOrderRevenueEntryAsync(Guid orderId)
        => context.FinanceEntries
            .FirstOrDefaultAsync(x =>
                x.OrderId == orderId &&
                x.SourceType == FinanceEntrySourceType.Order);

    public async Task<decimal> GetOrderFinanceTotalAsync(Guid orderId)
    {
        return await context.FinanceEntries
            .Where(x => x.OrderId == orderId)
            .SumAsync(x => (decimal?)x.Amount) ?? 0m;
    }

    public Task<bool> AnyByOrderIdAsync(Guid orderId)
    {
        return context.FinanceEntries.AnyAsync(x => x.OrderId == orderId);
    }

    public Task<List<FinanceEntry>> GetByOrderIdAsync(Guid orderId)
    {
        return context.FinanceEntries
            .Where(x => x.OrderId == orderId)
            .ToListAsync();
    }
}