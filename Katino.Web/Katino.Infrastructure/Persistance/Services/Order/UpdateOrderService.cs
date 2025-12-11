using Katino.Domain.Entities;
using Katino.Domain.Models;
using Katino.Domain.Services.OrderN.UpdateOrderService;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class UpdateOrderService : IUpdateOrderService
{
    public async Task<OrderUpdateResult> UpdateAsync(Order order)
    {
        // TODO implement
        throw new NotImplementedException();
    }
}
