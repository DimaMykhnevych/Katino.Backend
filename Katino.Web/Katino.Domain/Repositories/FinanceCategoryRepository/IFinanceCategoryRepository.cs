using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Repositories.FinanceCategoryRepository;

public interface IFinanceCategoryRepository : IRepository<FinanceCategory>
{
    Task<FinanceCategory> GetByTypeAndNameAsync(FinanceCategoryType type, string name);
    Task<List<FinanceCategory>> GetActiveByTypeAsync(FinanceCategoryType type);
    Task<bool> ExistsActiveByTypeAndNameAsync(FinanceCategoryType type, string name);
}
