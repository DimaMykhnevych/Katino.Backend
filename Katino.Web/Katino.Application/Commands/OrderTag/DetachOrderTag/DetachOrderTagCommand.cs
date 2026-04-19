using MediatR;

namespace Katino.Application.Commands.OrderTag.DetachOrderTag;

public class DetachOrderTagCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public Guid TagId { get; set; }
}
