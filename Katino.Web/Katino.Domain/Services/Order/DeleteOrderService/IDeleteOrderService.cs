using Katino.Domain.Models;

namespace Katino.Domain.Services.OrderN.DeleteOrderService;

public interface IDeleteOrderService
{
    Task<OrderDeleteResult> DeleteAsync(Guid id);
}
