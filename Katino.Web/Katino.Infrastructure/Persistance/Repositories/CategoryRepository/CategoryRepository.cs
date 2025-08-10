using Katino.Domain.Entities;
using Katino.Domain.Repositories.CategoryRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.CategoryRepository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(KatinoDbContext context) : base(context)
    {
    }
}
