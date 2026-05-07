using MediatR;

namespace Katino.Application.Commands.User.SetUserActivationStatus;

public class SetUserActivationStatusCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}
