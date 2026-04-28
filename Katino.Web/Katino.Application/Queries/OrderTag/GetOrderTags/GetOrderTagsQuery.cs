using Katino.Application.DTOs.OrderTag;
using MediatR;

namespace Katino.Application.Queries.OrderTag.GetOrderTags;

public class GetOrderTagsQuery : IRequest<IEnumerable<OrderTagDto>>
{
    public string Search { get; set; }
    public bool? CustomOnly { get; set; }
}
