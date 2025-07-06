using Katino.Domain.Entities;
using Katino.Domain.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.User.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public DeleteUserCommandHandler(
        UserManager<AppUser> userManager,
        ILoggerFactory loggerFactory)
    {
        _userManager = userManager;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteUserCommandHandler));
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete user request");
        ArgumentNullException.ThrowIfNull(request);

        AppUser userToDelete = await _userManager.FindByIdAsync(request.Id.ToString());
        if (userToDelete == null)
        {
            _logger.LogWarning("User with given Id {userId} was not found in the database", request.Id);
            return false;
        }

        IdentityResult deleteUserResult = await _userManager.DeleteAsync(userToDelete);

        deleteUserResult.ValidateIdentityResult();

        return true;
    }
}