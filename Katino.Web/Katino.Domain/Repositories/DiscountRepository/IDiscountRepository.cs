using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.DiscountRepository;

public interface IDiscountRepository : IRepository<Discount>
{
    Task<List<Discount>> GetAllWithDetailsAsync();
    Task<Discount> GetWithDetailsAsync(Guid id);
    Task<List<Discount>> GetActiveWithDetailsAsync();
}
