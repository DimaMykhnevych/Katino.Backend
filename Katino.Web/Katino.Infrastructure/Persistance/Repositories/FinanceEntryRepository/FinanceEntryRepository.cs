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
}