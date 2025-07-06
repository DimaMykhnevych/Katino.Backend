using Katino.Application.DTOs;
using Katino.Application.Services.AuthorizationService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.Auth.SignIn;

public class SignInCommandHandler : IRequestHandler<SignInCommand, JWTTokenStatusResultDto>
{
    private readonly BaseAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public SignInCommandHandler(BaseAuthorizationService authorizationService, ILoggerFactory loggerFactory)
    {
        _authorizationService = authorizationService;
        _logger = loggerFactory?.CreateLogger(nameof(SignInCommandHandler));
    }

    public async Task<JWTTokenStatusResultDto> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling sign in request");
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Generating token for user {username}", request.UserName);
        return await _authorizationService.GenerateTokenAsync(request);
    }
}