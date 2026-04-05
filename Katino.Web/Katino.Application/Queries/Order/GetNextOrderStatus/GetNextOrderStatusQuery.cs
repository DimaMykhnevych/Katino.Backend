using MediatR;
using Katino.Application.DTOs.Order;

namespace Katino.Application.Queries.OrderN.GetNextOrderStatus;

public class GetNextOrderStatusQuery : IRequest<OrderStatusDto[]>
{
    public Guid OrderId { get; set; }
}
