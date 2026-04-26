using AutoMapper;
using Katino.Application.DTOs.OrderItem;
using Katino.Domain.Services.OrderItemN.SewingQueueService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.OrderItemN.GetSewingQueue;

public class GetSewingQueueQueryHandler : IRequestHandler<GetSewingQueueQuery, GetSewingQueueItemsDto>
{
    private readonly ISewingQueueService _sewingQueueService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetSewingQueueQueryHandler(
        ISewingQueueService sewingQueueService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _sewingQueueService = sewingQueueService;
        _logger = loggerFactory?.CreateLogger(nameof(GetSewingQueueQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetSewingQueueItemsDto> Handle(GetSewingQueueQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get sewing queue");
        ArgumentNullException.ThrowIfNull(request);

        var sewingQueue = await _sewingQueueService.GetSewingQueueAsync(request.SewerId, cancellationToken);

        List<SewingQueueItemDto> sewingQueueItems =
            _mapper.Map<IEnumerable<SewingQueueItemDto>>(sewingQueue)
                .ToList();

        return new GetSewingQueueItemsDto
        {
            SewingQueueItems = sewingQueueItems,
            ResultsAmount = sewingQueueItems.Count
        };
    }
}
