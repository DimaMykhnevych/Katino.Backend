using MediatR;

namespace Katino.Application.Commands.User.ConfirmEmail;

public class ConfirmEmailCommand : IRequest<bool>
{
    public string Token { get; set; }
    public string Email { get; set; }
}
