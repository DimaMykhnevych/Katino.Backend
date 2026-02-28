using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.FinanceEntryRepository;

public interface IFinanceEntryRepository : IRepository<FinanceEntry>
{
    Task<FinanceEntry> GetOrderRevenueEntryAsync(Guid orderId);
    Task<decimal> GetOrderFinanceTotalAsync(Guid orderId);
    Task<bool> AnyByOrderIdAsync(Guid orderId);
    Task<List<FinanceEntry>> GetByOrderIdAsync(Guid orderId);
    Task<string?> GetAnyTtnByOrderIdAsync(Guid orderId);
}
