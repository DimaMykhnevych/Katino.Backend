using Katino.Domain.Enums;

namespace Katino.Domain.Services.OrderN.SetOrderManualStatusService;

public interface ISetOrderManualStatusService
{
    Task<OrderStatus[]> GetNextOrderStatusesAsync(Guid orderId);
    Task<bool> SetOrderManualStatusAsync(Guid orderId, OrderStatus orderStatus);
}
