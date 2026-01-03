using AutoMapper;
using Katino.Domain.Enums;
using Katino.Domain.Services.OrderN.SetOrderManualStatusService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.OrderN.SetOrderManualStatus;

public class SetOrderManualStatusCommandHandler : IRequestHandler<SetOrderManualStatusCommand, bool>
{
    private readonly ISetOrderManualStatusService _setOrderManualStatusService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public SetOrderManualStatusCommandHandler(
        ISetOrderManualStatusService setOrderManualStatusService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _setOrderManualStatusService = setOrderManualStatusService;
        _logger = loggerFactory?.CreateLogger(nameof(SetOrderManualStatusCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(SetOrderManualStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling set order manual status request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            OrderManualStatus orderManualStatus = _mapper.Map<OrderManualStatus>(request.OrderManualStatus);
            return await _setOrderManualStatusService.SetOrderManualStatusAsync(request.OrderId, orderManualStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during setting order manual status");
            return false;
        }
    }
}
