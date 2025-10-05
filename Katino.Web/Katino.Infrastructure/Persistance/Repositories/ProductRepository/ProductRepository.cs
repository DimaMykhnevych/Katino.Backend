using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.ProductRepository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<Product> GetProductWithCategoryAsync(Guid id)
    {
        return await context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
