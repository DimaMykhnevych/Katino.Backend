using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Models;

namespace Katino.Domain.Services.OrderN.DeleteOrderService;

public interface IDeleteOrderService
{
    Task<OrderDeleteResult> DeleteAsync(Guid id);
    Task HandleOrderRejectionAsync(Order order, OrderInternetDocStatus orderInternalDocStatus);
}
