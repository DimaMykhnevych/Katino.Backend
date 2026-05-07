using Katino.Application.DTOs.User;
using Katino.Domain.Builders;
using Katino.Domain.Constants;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.User.GetUsersForActivation;

public class GetUsersForActivationQueryHandler : IRequestHandler<GetUsersForActivationQuery, IEnumerable<ManageableUserDto>>
{
    private readonly IAppUserQueryBuilder _userQueryBuilder;
    private readonly ILogger _logger;

    public GetUsersForActivationQueryHandler(IAppUserQueryBuilder userQueryBuilder, ILoggerFactory loggerFactory)
    {
        _userQueryBuilder = userQueryBuilder;
        _logger = loggerFactory?.CreateLogger(nameof(GetUsersForActivationQueryHandler));
    }

    public async Task<IEnumerable<ManageableUserDto>> Handle(GetUsersForActivationQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get users for activation request");
        ArgumentNullException.ThrowIfNull(request);

        string[] excludedRoles = request.CallerRole == Role.Admin
            ? [Role.Admin]
            : [Role.Admin, Role.Owner];

        IEnumerable<AppUser> users = _userQueryBuilder
            .SetBaseUserInfo()
            .ExcludeRoles(excludedRoles)
            .Build();

        return users.Select(u => new ManageableUserDto
        {
            UserId = u.Id,
            UserName = u.UserName,
            Role = u.Role,
            RegistryDate = u.RegistryDate,
            Email = u.Email,
            IsActive = !u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTimeOffset.UtcNow
        });
    }
}
