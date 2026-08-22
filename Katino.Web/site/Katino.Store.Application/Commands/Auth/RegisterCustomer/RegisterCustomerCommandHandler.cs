using Katino.Store.Application.Services.AuthorizationService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Commands.Auth.RegisterCustomer;

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, bool>
{
    private readonly ICustomerAuthorizationService _customerAuthorizationService;
    private readonly ILogger _logger;

    public RegisterCustomerCommandHandler(ICustomerAuthorizationService customerAuthorizationService, ILoggerFactory loggerFactory)
    {
        _customerAuthorizationService = customerAuthorizationService;
        _logger = loggerFactory?.CreateLogger(nameof(RegisterCustomerCommandHandler));
    }

    public async Task<bool> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling customer registration request");
        ArgumentNullException.ThrowIfNull(request);

        await _customerAuthorizationService.RegisterAsync(
            request.Email,
            request.Password,
            request.ConfirmPassword,
            request.ClientUriForEmailConfirmation);

        return true;
    }
}
