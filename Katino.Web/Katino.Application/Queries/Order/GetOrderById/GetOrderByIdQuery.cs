using Katino.Application.DTOs.Order;
using MediatR;

namespace Katino.Application.Queries.OrderN.GetOrderById;

public class GetOrderByIdQuery : IRequest<OrderDto>
{
    public Guid Id { get; set; }
}
