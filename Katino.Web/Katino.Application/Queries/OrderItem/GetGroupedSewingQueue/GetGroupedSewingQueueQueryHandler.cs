using AutoMapper;
using Katino.Application.DTOs.OrderItem;
using Katino.Domain.Services.OrderItemN.SewingQueueService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.OrderItemN.GetGroupedSewingQueue;

public class GetGroupedSewingQueueQueryHandler : IRequestHandler<GetGroupedSewingQueueQuery, List<GroupedSewingQueueItemDto>>
{
    private readonly ISewingQueueService _sewingQueueService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetGroupedSewingQueueQueryHandler(
        ISewingQueueService sewingQueueService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _sewingQueueService = sewingQueueService;
        _logger = loggerFactory?.CreateLogger(nameof(GetGroupedSewingQueueQueryHandler));
        _mapper = mapper;
    }

    public async Task<List<GroupedSewingQueueItemDto>> Handle(GetGroupedSewingQueueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get grouped sewing queue");
        ArgumentNullException.ThrowIfNull(request);

        var sewingQueue = await _sewingQueueService.GetSewingQueueGroupedByDateAsync(request.SewerId, cancellationToken);
        return sewingQueue.Select(i => new GroupedSewingQueueItemDto
        {
            SendUntilDate = i.Key,
            Items = _mapper.Map<List<SewingQueueItemDto>>(i.Value)
                .ToList(),
        }).ToList();
    }
}
