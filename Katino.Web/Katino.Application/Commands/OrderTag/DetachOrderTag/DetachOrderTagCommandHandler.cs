using Katino.Domain.Repositories.OrderTagRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.OrderTag.DetachOrderTag;

public class DetachOrderTagCommandHandler : IRequestHandler<DetachOrderTagCommand, bool>
{
    private readonly IOrderTagRepository _orderTagRepository;
    private readonly ILogger _logger;

    public DetachOrderTagCommandHandler(
        IOrderTagRepository orderTagRepository,
        ILoggerFactory loggerFactory)
    {
        _orderTagRepository = orderTagRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DetachOrderTagCommandHandler));
    }

    public async Task<bool> Handle(DetachOrderTagCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling detach order tag request for order {OrderId}, tag {TagId}", request.OrderId, request.TagId);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _orderTagRepository.DetachTagFromOrderAsync(request.OrderId, request.TagId);
            await _orderTagRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during detaching tag {TagId} from order {OrderId}", request.TagId, request.OrderId);
            return false;
        }
    }
}
