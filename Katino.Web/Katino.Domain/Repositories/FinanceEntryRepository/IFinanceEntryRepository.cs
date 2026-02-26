using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.FinanceEntryRepository;

public interface IFinanceEntryRepository : IRepository<FinanceEntry>
{
    Task<FinanceEntry> GetOrderRevenueEntryAsync(Guid orderId);
}
