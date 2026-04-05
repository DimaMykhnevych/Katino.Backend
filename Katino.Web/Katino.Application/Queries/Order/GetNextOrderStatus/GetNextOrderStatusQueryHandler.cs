using AutoMapper;
using Katino.Application.DTOs.Order;
using Katino.Domain.Services.OrderN.SetOrderManualStatusService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.OrderN.GetNextOrderStatus;

public class GetNextOrderStatusQueryHandler : IRequestHandler<GetNextOrderStatusQuery, OrderStatusDto[]>
{
    private readonly ISetOrderManualStatusService _orderManualStatusService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetNextOrderStatusQueryHandler(
        ISetOrderManualStatusService orderManualStatusService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _orderManualStatusService = orderManualStatusService;
        _logger = loggerFactory?.CreateLogger(nameof(GetNextOrderStatusQueryHandler));
        _mapper = mapper;
    }

    public async Task<OrderStatusDto[]> Handle(GetNextOrderStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get next order status");
        ArgumentNullException.ThrowIfNull(request);

        var statuses = await _orderManualStatusService.GetNextOrderStatusesAsync(request.OrderId);

        return _mapper.Map<OrderStatusDto[]>(statuses);
    }
}
