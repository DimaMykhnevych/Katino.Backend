using AutoMapper;
using Katino.Application.DTOs.Order;
using Katino.Domain.Models;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.OrderN.DeleteOrder;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, OrderDeleteResultDto>
{
    private readonly IDeleteOrderService _deleteOrderService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public DeleteOrderCommandHandler(
        IDeleteOrderService deleteOrderService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _deleteOrderService = deleteOrderService;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteOrderCommandHandler));
        _mapper = mapper;
    }

    public async Task<OrderDeleteResultDto> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling delete order request with order id {request.Id}");
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            OrderDeleteResult deleteResult = await _deleteOrderService.DeleteAsync(request.Id).ConfigureAwait(false);
            return _mapper.Map<OrderDeleteResultDto>(deleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occured during deleting order {request.Id}");
            return new();
        }
    }
}
