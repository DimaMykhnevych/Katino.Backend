using Katino.Store.Application.DTOs;
using Katino.Store.Application.Services.AuthorizationService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Commands.Auth.SignInCustomer;

public class SignInCustomerCommandHandler : IRequestHandler<SignInCustomerCommand, CustomerAuthResultDto>
{
    private readonly ICustomerAuthorizationService _customerAuthorizationService;
    private readonly ILogger _logger;

    public SignInCustomerCommandHandler(ICustomerAuthorizationService customerAuthorizationService, ILoggerFactory loggerFactory)
    {
        _customerAuthorizationService = customerAuthorizationService;
        _logger = loggerFactory?.CreateLogger(nameof(SignInCustomerCommandHandler));
    }

    public async Task<CustomerAuthResultDto> Handle(SignInCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling customer sign in request");
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Generating token for customer {email}", request.Email);
        return await _customerAuthorizationService.SignInAsync(request.Email, request.Password);
    }
}
