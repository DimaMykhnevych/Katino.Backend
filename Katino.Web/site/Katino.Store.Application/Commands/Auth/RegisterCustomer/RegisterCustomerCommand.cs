using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Katino.Store.Application.Commands.Auth.RegisterCustomer;

public class RegisterCustomerCommand : IRequest<bool>
{
    [Required(ErrorMessage = "emailRequired")]
    [EmailAddress(ErrorMessage = "emailInvalidFormat")]
    public string Email { get; set; }

    [Required(ErrorMessage = "passwordRequired")]
    public string Password { get; set; }

    [Required(ErrorMessage = "confirmPasswordRequired")]
    public string ConfirmPassword { get; set; }

    public string ClientUriForEmailConfirmation { get; set; }
}
