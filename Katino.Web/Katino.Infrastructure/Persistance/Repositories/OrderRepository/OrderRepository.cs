using Katino.Domain.Entities;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.OrderRepository;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(KatinoDbContext context) : base(context)
    {
    }
}
