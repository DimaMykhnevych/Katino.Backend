using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.ProductRepository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(KatinoDbContext context) : base(context)
    {
    }
}
