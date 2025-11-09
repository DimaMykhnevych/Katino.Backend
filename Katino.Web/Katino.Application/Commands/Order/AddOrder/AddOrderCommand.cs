using Katino.Application.DTOs.Order;
using MediatR;

namespace Katino.Application.Commands.OrderN.AddOrder;

public class AddOrderCommand : IRequest<OrderCreationResultDto>
{
    public AddOrderDto Order { get; set; }
    public AddNovaPostInternetDocumentDto NovaPostInternetDocument {get; set;}
}
