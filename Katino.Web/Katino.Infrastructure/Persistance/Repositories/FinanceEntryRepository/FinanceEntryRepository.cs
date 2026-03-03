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

    public async Task<string> GetAnyTtnByOrderIdAsync(Guid orderId)
    {
        return await context.FinanceEntries
            .Where(x => x.OrderId == orderId && x.InternetDocumentIntDocNumber != null)
            .Select(x => x.InternetDocumentIntDocNumber)
            .FirstOrDefaultAsync();
    }

    public Task<FinanceEntry> GetByIdAsync(Guid id, CancellationToken ct = default)
        => context.Set<FinanceEntry>()
        .Include(x => x.Category)
        .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<FinanceEntry>> GetManualExpensesByYearAsync(int year, CancellationToken ct = default)
        => context.Set<FinanceEntry>()
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x =>
                x.SourceType == FinanceEntrySourceType.Manual &&
                x.Reason == FinanceEntryReason.Expense &&
                x.EntryDate.Year == year &&
                x.Category.Type == FinanceCategoryType.Expense)
            .OrderByDescending(x => x.EntryDate)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

    public IQueryable<FinanceEntry> Query() => context.FinanceEntries;
}