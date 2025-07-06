using MediatR;

namespace Katino.Application.Commands.User.DeleteUser;

public class DeleteUserCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
