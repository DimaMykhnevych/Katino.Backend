using Katino.Application.DTOs.Order;
using MediatR;

namespace Katino.Application.Queries.OrderN.GetNextOrderStatus;

public class GetNextOrderStatusQuery : IRequest<OrderStatusDto[]>
{
    public OrderStatusDto CurrentOrderStatus { get; set; }
}
