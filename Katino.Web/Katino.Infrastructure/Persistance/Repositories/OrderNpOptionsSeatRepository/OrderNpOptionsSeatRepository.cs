using Katino.Domain.Entities;
using Katino.Domain.Repositories.OrderNpOptionsSeatRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.OrderNpOptionsSeatRepository;

public class OrderNpOptionsSeatRepository : Repository<OrderNpOptionsSeat>, IOrderNpOptionsSeatRepository
{
    public OrderNpOptionsSeatRepository(KatinoDbContext context) : base(context)
    {
    }
}
