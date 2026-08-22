using Katino.Store.Application.DTOs;

namespace Katino.Store.Application.Services.AuthorizationService;

public interface ICustomerAuthorizationService
{
    Task RegisterAsync(string email, string password, string confirmPassword, string clientUriForEmailConfirmation);
    Task ConfirmEmailAsync(string email, string token);
    Task<CustomerAuthResultDto> SignInAsync(string email, string password);
}
