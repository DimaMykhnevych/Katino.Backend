using Katino.Domain.Entities;
using Katino.Domain.Models;

namespace Katino.Domain.Services.OrderN.UpdateOrderService;

public interface IUpdateOrderService
{
    Task<OrderUpdateResult> UpdateAsync(Order order);
}
