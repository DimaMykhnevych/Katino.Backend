using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.FinanceCategoryRepository;

public class FinanceCategoryRepository : Repository<FinanceCategory>, IFinanceCategoryRepository
{
    public FinanceCategoryRepository(KatinoDbContext context) : base(context) { }

    public async Task<FinanceCategory> GetByTypeAndNameAsync(FinanceCategoryType type, string name)
    {
        return await context.FinanceCategories.FirstOrDefaultAsync(x =>
            x.Type == type &&
            x.Name == name);
    }
}
