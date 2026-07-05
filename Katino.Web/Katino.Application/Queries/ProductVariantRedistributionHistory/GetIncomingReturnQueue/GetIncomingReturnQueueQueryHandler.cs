using AutoMapper;
using Katino.Application.DTOs.OrderItem;
using Katino.Domain.Services.ProductVariantRedistributionN.IncomingReturnQueueService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.ProductVariantRedistributionHistoryN.GetIncomingReturnQueue;

public class GetIncomingReturnQueueQueryHandler : IRequestHandler<GetIncomingReturnQueueQuery, List<GroupedSewingQueueItemDto>>
{
    private readonly IIncomingReturnQueueService _incomingReturnQueueService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetIncomingReturnQueueQueryHandler(
        IIncomingReturnQueueService incomingReturnQueueService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _incomingReturnQueueService = incomingReturnQueueService;
        _logger = loggerFactory?.CreateLogger(nameof(GetIncomingReturnQueueQueryHandler));
        _mapper = mapper;
    }

    public async Task<List<GroupedSewingQueueItemDto>> Handle(GetIncomingReturnQueueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get incoming return queue");

        var queue = await _incomingReturnQueueService.GetIncomingReturnQueueGroupedByDateAsync(cancellationToken);
        return queue.Select(i => new GroupedSewingQueueItemDto
        {
            SendUntilDate = i.Key,
            Items = _mapper.Map<List<SewingQueueItemDto>>(i.Value)
        }).ToList();
    }
}
