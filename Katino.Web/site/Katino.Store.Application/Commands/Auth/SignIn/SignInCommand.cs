using Katino.Store.Application.DTOs;
using MediatR;

namespace Katino.Store.Application.Commands.Auth.SignIn;

public class SignInCommand : IRequest<JWTTokenStatusResultDto>
{
    public string UserName { get; set; }

    public string Password { get; set; }
}