using Katino.Domain.Entities;
using Katino.Domain.Repositories.CustomerRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.CustomerRepository;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(KatinoDbContext context) : base(context) { }

    public async Task<Customer> GetByEmailAsync(string normalizedEmail)
    {
        return await context.Customers.FirstOrDefaultAsync(c => c.Email == normalizedEmail);
    }
}
