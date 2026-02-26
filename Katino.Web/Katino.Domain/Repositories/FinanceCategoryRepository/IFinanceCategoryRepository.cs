using Katino.Domain.Entities;
using Katino.Domain.Enums;

namespace Katino.Domain.Repositories.FinanceCategoryRepository;

public interface IFinanceCategoryRepository : IRepository<FinanceCategory>
{
    Task<FinanceCategory> GetByTypeAndNameAsync(FinanceCategoryType type, string name);
}
