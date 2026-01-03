using Katino.Application.DTOs.Order;
using MediatR;

namespace Katino.Application.Commands.OrderN.SetOrderManualStatus;

public class SetOrderManualStatusCommand : IRequest<bool>
{
    public Guid OrderId { get; set; }
    public OrderManualStatusDto OrderManualStatus { get; set; }
}
