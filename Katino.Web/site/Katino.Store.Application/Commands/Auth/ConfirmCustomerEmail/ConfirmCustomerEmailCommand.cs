using MediatR;

namespace Katino.Store.Application.Commands.Auth.ConfirmCustomerEmail;

public class ConfirmCustomerEmailCommand : IRequest<bool>
{
    public string Email { get; set; }

    public string Token { get; set; }
}
