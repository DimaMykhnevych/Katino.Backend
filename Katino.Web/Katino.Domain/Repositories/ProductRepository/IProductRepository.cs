using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.ProductRepository;

public interface IProductRepository : IRepository<Product>
{
    Task<Product> GetProductWithCategoryAsync(Guid id);
}
