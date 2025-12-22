using Katino.Domain.Entities;
using Katino.Domain.Repositories.OrderAddressInfoRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.OrderAddressInfoRepository;

public class OrderAddressInfoRepository : Repository<OrderAddressInfo>, IOrderAddressInfoRepository
{
    public OrderAddressInfoRepository(KatinoDbContext context) : base(context)
    {
    }
}
