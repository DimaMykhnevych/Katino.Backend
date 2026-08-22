using Katino.Store.Application.DTOs;
using MediatR;

namespace Katino.Store.Application.Commands.Auth.SignInCustomer;

public class SignInCustomerCommand : IRequest<CustomerAuthResultDto>
{
    public string Email { get; set; }

    public string Password { get; set; }
}
