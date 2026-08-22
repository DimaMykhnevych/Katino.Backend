using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.CustomerRepository;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer> GetByEmailAsync(string normalizedEmail);
}
