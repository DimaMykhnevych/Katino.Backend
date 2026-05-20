using Katino.Domain.Entities;

namespace Katino.Domain.Services.TelegramN;

public interface IOrderRejectionNotifier
{
    Task NotifyAsync(Order order);
}
