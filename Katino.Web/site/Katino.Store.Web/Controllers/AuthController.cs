using Katino.Store.Application.Commands.Auth.SignIn;
using Katino.Store.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Katino.Store.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    [AllowAnonymous]
    [Route("token")]
    [EnableRateLimiting("auth")]
    [SwaggerOperation(Summary = "Gets a Bearer token with basic user info in case of successful authorization",
        Description = "As a result returns the model with LoginErrorCode property.\n\nPossible LoginErrorCode values:\n\n" +
        "0 = Invalid userName or password\n\n" +
        "1 = Email confirmation required\n\n" +
        "100 = User was successfully authorized")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(JWTTokenStatusResultDto))]
    public async Task<IActionResult> Login([FromBody] SignInCommand command)
    {
        JWTTokenStatusResultDto result = await _mediator.Send(command);
        return Ok(result);
    }

}
