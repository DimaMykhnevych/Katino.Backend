using AutoMapper;
using Katino.Application.DTOs.Order;
using Katino.Domain.Builders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.OrderN.GetOrder;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, GetOrderDto>
{
    private readonly IOrderQueryBuilder _orderQueryBuilder;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetOrderQueryHandler(
        IOrderQueryBuilder orderQueryBuilder,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _orderQueryBuilder = orderQueryBuilder;
        _logger = loggerFactory?.CreateLogger(nameof(GetOrderQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetOrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get orders");
        ArgumentNullException.ThrowIfNull(request);

        var orders = _orderQueryBuilder
            .SetBaseOrderInfo()
            .ApplySearch(request.Search)
            .ApplyPaging(request.Page, request.PageSize)
            .Build();

        var ordersCount = await _orderQueryBuilder
            .SetBaseOrderInfoForToatalCount()
            .ApplySearch(request.Search)
            .Build()
            .CountAsync(cancellationToken);

        var resultOrders = await orders.ToListAsync(cancellationToken);

        List<OrderDto> orderDtos =
            _mapper.Map<IEnumerable<OrderDto>>(resultOrders)
                .ToList();

        return new GetOrderDto
        {
            Orders = orderDtos,
            ResultsAmount = ordersCount
        };
    }
}
