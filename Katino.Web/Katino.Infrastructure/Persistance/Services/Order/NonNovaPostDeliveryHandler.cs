using Katino.Domain.Entities;
using Katino.Domain.Services.OrderN.OrderDeliveryHandler;

namespace Katino.Infrastructure.Persistance.Services.OrderN;

public class NonNovaPostDeliveryHandler : IOrderDeliveryHandler
{
    public Task<Guid?> ResolveOrderRecipientAsync(Order order)
        => Task.FromResult<Guid?>(null);

    public Task<List<OrderNpOptionsSeat>> ResolveNpOptionSeatsAsync(IEnumerable<OrderNpOptionsSeat> requestedSeats)
        => Task.FromResult(new List<OrderNpOptionsSeat>());

    public Task<bool> HandleInternetDocumentOnAddAsync(Order insertedOrder)
        => Task.FromResult(true);

    public Task<bool> HandleInternetDocumentOnUpdateAsync(Order updatedOrder, Order currentOrderInDb)
        => Task.FromResult(true);

    public Task<bool> HandleInternetDocumentOnDeleteAsync(Order order)
        => Task.FromResult(true);
}
