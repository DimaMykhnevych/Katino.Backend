using Katino.Application.DTOs.User;
using Katino.Domain.Builders;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.User.GetSewers;

public class GetSewersQueryHandler : IRequestHandler<GetSewersQuery, List<SewerDto>>
{
    private readonly IAppUserQueryBuilder _userQueryBuilder;
    private readonly ILogger _logger;

    public GetSewersQueryHandler(IAppUserQueryBuilder userQueryBuilder, ILoggerFactory loggerFactory)
    {
        _userQueryBuilder = userQueryBuilder;
        _logger = loggerFactory?.CreateLogger(nameof(GetSewersQueryHandler));
    }

    public Task<List<SewerDto>> Handle(GetSewersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get sewers request");

        var sewers = _userQueryBuilder
            .SetBaseUserInfo()
            .SetRole(Role.Sewer)
            .Build()
            .Select(u => new SewerDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email
            })
            .ToList();

        return Task.FromResult(sewers);
    }
}
