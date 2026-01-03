using Katino.Domain.Enums;

namespace Katino.Domain.Services.OrderN.SetOrderManualStatusService;

public interface ISetOrderManualStatusService
{
    Task<bool> SetOrderManualStatusAsync(Guid orderId, OrderManualStatus orderManualStatus);
}
