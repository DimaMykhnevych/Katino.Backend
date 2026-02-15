using Katino.Application.DTOs.OrderItem;
using MediatR;

namespace Katino.Application.Queries.OrderItemN.GetGroupedSewingQueue;

public class GetGroupedSewingQueueQuery : IRequest<List<GroupedSewingQueueItemDto>>
{
}
