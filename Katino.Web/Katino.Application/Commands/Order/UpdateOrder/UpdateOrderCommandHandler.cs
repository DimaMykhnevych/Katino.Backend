using AutoMapper;
using Katino.Application.DTOs.Order;
using Katino.Domain.Entities;
using Katino.Domain.Models;
using Katino.Domain.Services.OrderN.UpdateOrderService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.OrderN.UpdateOrder;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderUpdateResultDto>
{
    private readonly IUpdateOrderService _updateOrderService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateOrderCommandHandler(
        IUpdateOrderService updateOrderService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _updateOrderService = updateOrderService;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateOrderCommandHandler));
        _mapper = mapper;
    }

    public async Task<OrderUpdateResultDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update order request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Order order = _mapper.Map<Order>(request);

            OrderUpdateResult updateResult = await _updateOrderService.UpdateAsync(order, request.CustomTags).ConfigureAwait(false);
            return _mapper.Map<OrderUpdateResultDto>(updateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating order");
            return new();
        }
    }
}
