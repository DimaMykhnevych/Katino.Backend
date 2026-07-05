using Katino.Application.DTOs.OrderItem;
using MediatR;

namespace Katino.Application.Queries.ProductVariantRedistributionHistoryN.GetIncomingReturnQueue;

public class GetIncomingReturnQueueQuery : IRequest<List<GroupedSewingQueueItemDto>>
{
}
