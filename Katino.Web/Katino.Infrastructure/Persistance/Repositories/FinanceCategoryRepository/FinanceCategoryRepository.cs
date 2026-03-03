using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.FinanceCategoryRepository;

public class FinanceCategoryRepository : Repository<FinanceCategory>, IFinanceCategoryRepository
{
    public FinanceCategoryRepository(KatinoDbContext context) : base(context) { }

    public async Task<bool> ExistsActiveByTypeAndNameAsync(FinanceCategoryType type, string name)
    {
        return await context.FinanceCategories
            .AnyAsync(x => x.Type == type && x.Name == name && x.IsActive);
    }

    public async Task<List<FinanceCategory>> GetActiveByTypeAsync(FinanceCategoryType type)
    {
        return await context.FinanceCategories
            .AsNoTracking()
            .Where(x => x.Type == type && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<FinanceCategory> GetByTypeAndNameAsync(FinanceCategoryType type, string name)
    {
        return await context.FinanceCategories.FirstOrDefaultAsync(x =>
            x.Type == type &&
            x.Name == name);
    }

    public IQueryable<FinanceCategory> Query() => context.FinanceCategories;
}
