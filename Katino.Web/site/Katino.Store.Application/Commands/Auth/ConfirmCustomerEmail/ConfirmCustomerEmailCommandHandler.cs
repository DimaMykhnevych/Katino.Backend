using Katino.Store.Application.Services.AuthorizationService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Store.Application.Commands.Auth.ConfirmCustomerEmail;

public class ConfirmCustomerEmailCommandHandler : IRequestHandler<ConfirmCustomerEmailCommand, bool>
{
    private readonly ICustomerAuthorizationService _customerAuthorizationService;
    private readonly ILogger _logger;

    public ConfirmCustomerEmailCommandHandler(ICustomerAuthorizationService customerAuthorizationService, ILoggerFactory loggerFactory)
    {
        _customerAuthorizationService = customerAuthorizationService;
        _logger = loggerFactory?.CreateLogger(nameof(ConfirmCustomerEmailCommandHandler));
    }

    public async Task<bool> Handle(ConfirmCustomerEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling customer email confirmation request");
        ArgumentNullException.ThrowIfNull(request);

        await _customerAuthorizationService.ConfirmEmailAsync(request.Email, request.Token);

        return true;
    }
}
