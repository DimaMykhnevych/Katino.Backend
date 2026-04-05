using Katino.Domain.Enums.NovaPost;

namespace Katino.Domain.Services.OrderN.OrderDeliveryHandler;

public interface IOrderDeliveryHandlerFactory
{
    IOrderDeliveryHandler Create(DeliveryType deliveryType);
}
