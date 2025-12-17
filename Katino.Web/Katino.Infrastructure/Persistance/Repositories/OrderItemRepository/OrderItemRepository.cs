using Katino.Domain.Entities;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.OrderItemRepository;

public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(KatinoDbContext context) : base(context)
    {
    }
}
