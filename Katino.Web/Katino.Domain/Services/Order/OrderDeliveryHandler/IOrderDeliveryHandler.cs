using Katino.Domain.Entities;

namespace Katino.Domain.Services.OrderN.OrderDeliveryHandler;

public interface IOrderDeliveryHandler
{
    Task<Guid?> ResolveOrderRecipientAsync(Order order);
    Task<List<OrderNpOptionsSeat>> ResolveNpOptionSeatsAsync(IEnumerable<OrderNpOptionsSeat> requestedSeats);
    Task<bool> HandleInternetDocumentOnAddAsync(Order insertedOrder);
    Task<bool> HandleInternetDocumentOnUpdateAsync(Order updatedOrder, Order currentOrderInDb);
    Task<bool> HandleInternetDocumentOnDeleteAsync(Order order);
}
