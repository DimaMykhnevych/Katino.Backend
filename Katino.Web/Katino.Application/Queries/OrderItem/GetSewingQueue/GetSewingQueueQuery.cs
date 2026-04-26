using Katino.Application.DTOs.OrderItem;
using MediatR;

namespace Katino.Application.Queries.OrderItemN.GetSewingQueue;

public class GetSewingQueueQuery : IRequest<GetSewingQueueItemsDto>
{
    public Guid? SewerId { get; set; }
}
