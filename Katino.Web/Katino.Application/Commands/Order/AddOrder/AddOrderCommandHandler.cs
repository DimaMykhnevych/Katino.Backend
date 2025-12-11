using AutoMapper;
using Katino.Application.DTOs.Order;
using Katino.Domain.Entities;
using Katino.Domain.Models;
using Katino.Domain.Services.OrderN.AddOrderService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.OrderN.AddOrder;

public class AddOrderCommandHandler : IRequestHandler<AddOrderCommand, OrderCreationResultDto>
{
    private readonly IAddOrderService _addOrderService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddOrderCommandHandler(
        IAddOrderService addOrderService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _addOrderService = addOrderService;
        _logger = loggerFactory?.CreateLogger(nameof(AddOrderCommandHandler));
        _mapper = mapper;
    }

    public async Task<OrderCreationResultDto> Handle(AddOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add order request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Order order = _mapper.Map<Order>(request);

            OrderCreationResult creationResult = await _addOrderService.AddAsync(order).ConfigureAwait(false);
            return _mapper.Map<OrderCreationResultDto>(creationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding order");
            return new();
        }
    }
}
