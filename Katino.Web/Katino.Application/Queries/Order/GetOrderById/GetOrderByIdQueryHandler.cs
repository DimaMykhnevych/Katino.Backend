using AutoMapper;
using Katino.Application.DTOs.Order;
using Katino.Domain.Builders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.OrderN.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderQueryBuilder _orderQueryBuilder;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(
        IOrderQueryBuilder orderQueryBuilder,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _orderQueryBuilder = orderQueryBuilder;
        _logger = loggerFactory?.CreateLogger(nameof(GetOrderByIdQueryHandler));
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get order by id");
        ArgumentNullException.ThrowIfNull(request);

        var order = await _orderQueryBuilder
            .SetBaseOrderInfo()
            .Build()
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        return _mapper.Map<OrderDto>(order);
    }
}
