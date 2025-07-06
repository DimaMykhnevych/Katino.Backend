using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Katino.Application.Commands.User.CreateUser;

public class CreateUserCommand : IRequest<bool>
{
    public string Role { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }

    [EmailAddress]
    public string Email { get; set; }
    public string ClientURIForEmailConfirmation { get; set; }
}