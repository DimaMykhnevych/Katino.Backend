using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Services.OrderN.OrderDeliveryHandler;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class OrderDeliveryHandlerFactory : IOrderDeliveryHandlerFactory
{
    private readonly NovaPostDeliveryHandler _novaPostHandler;
    private readonly NonNovaPostDeliveryHandler _nonNovaPostHandler;

    public OrderDeliveryHandlerFactory(
        NovaPostDeliveryHandler novaPostHandler,
        NonNovaPostDeliveryHandler nonNovaPostHandler)
    {
        _novaPostHandler = novaPostHandler;
        _nonNovaPostHandler = nonNovaPostHandler;
    }

    public IOrderDeliveryHandler Create(DeliveryType deliveryType)
        => deliveryType == DeliveryType.NotNovaPost ? _nonNovaPostHandler : _novaPostHandler;
}
