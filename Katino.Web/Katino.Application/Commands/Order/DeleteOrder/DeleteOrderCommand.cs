using Katino.Application.DTOs.Order;
using MediatR;

namespace Katino.Application.Commands.OrderN.DeleteOrder;

public class DeleteOrderCommand : IRequest<OrderDeleteResultDto>
{
    public Guid Id { get; set; }
}
