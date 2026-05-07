using Katino.Domain.Entities;
using Katino.Domain.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.User.SetUserActivationStatus;

public class SetUserActivationStatusCommandHandler : IRequestHandler<SetUserActivationStatusCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public SetUserActivationStatusCommandHandler(UserManager<AppUser> userManager, ILoggerFactory loggerFactory)
    {
        _userManager = userManager;
        _logger = loggerFactory?.CreateLogger(nameof(SetUserActivationStatusCommandHandler));
    }

    public async Task<bool> Handle(SetUserActivationStatusCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling set user activation status request");
        ArgumentNullException.ThrowIfNull(request);

        AppUser user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            _logger.LogWarning("User with Id {userId} was not found", request.UserId);
            return false;
        }

        if (request.IsActive)
        {
            (await _userManager.SetLockoutEndDateAsync(user, null)).ValidateIdentityResult();
        }
        else
        {
            if (!await _userManager.GetLockoutEnabledAsync(user))
            {
                (await _userManager.SetLockoutEnabledAsync(user, true)).ValidateIdentityResult();
            }
            (await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue)).ValidateIdentityResult();
        }

        return true;
    }
}
