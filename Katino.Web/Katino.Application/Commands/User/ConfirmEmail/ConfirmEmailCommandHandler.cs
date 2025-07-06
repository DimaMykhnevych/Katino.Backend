using Katino.Domain.Entities;
using Katino.Domain.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.User.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ConfirmEmailCommandHandler(UserManager<AppUser> userManager, ILoggerFactory loggerFactory)
    {
        _userManager = userManager;
        _logger = loggerFactory?.CreateLogger(nameof(ConfirmEmailCommandHandler));
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling confirmation email request");
        ArgumentNullException.ThrowIfNull(request);

        AppUser user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("User with given email was not found in the database");
            return false;
        }

        _logger.LogDebug("Confirming email address");
        var confirmResult = await _userManager.ConfirmEmailAsync(user, request.Token);
        confirmResult.ValidateIdentityResult();
        return true;
    }
}