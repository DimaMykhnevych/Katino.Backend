using Katino.Domain.Enums;

namespace Katino.Domain.Services.OrderN.SetOrderManualStatusService;

public interface ISetOrderManualStatusService
{
    OrderStatus[] GetNextOrderStatuses(OrderStatus orderStatusCurrent);
    Task<bool> SetOrderManualStatusAsync(Guid orderId, OrderStatus orderStatus);
}
