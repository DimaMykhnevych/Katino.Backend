using Katino.Application.DTOs.Order;
using MediatR;

namespace Katino.Application.Queries.OrderN.GetOrder;

public class GetOrderQuery : IRequest<GetOrderDto>
{
    public string Search { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IList<OrderStatusDto> OrderStatuses { get; set; } = [];
    public DateTimeOffset? CreatedFrom { get; set; }
    public DateTimeOffset? CreatedTo { get; set; }
}
