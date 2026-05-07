using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Katino.Application.Commands.User.ConfirmEmail;
using Katino.Application.Commands.User.CreateUser;
using Katino.Application.Commands.User.DeleteUser;
using Katino.Application.Commands.User.SetUserActivationStatus;
using Katino.Application.DTOs;
using Katino.Application.DTOs.User;
using Katino.Domain.Constants;
using Katino.Domain.Exceptions;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;
using Katino.Application.Queries.User.GetAppUser;
using Katino.Application.Queries.User.GetSewers;
using Katino.Application.Queries.User.GetUsersForActivation;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    [SwaggerOperation(Summary = "Gets a filtered list of users", Description = "All parameters should be passed within the URI as a query parameters")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<UserAuthInfoDto>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "User was not authorized")]
    public async Task<IActionResult> Get([FromQuery] GetAppUserQuery getUserQuery)
    {
        IEnumerable<UserAuthInfoDto> users = await _mediator.Send(getUserQuery);
        return Ok(users);
    }

    [HttpGet("sewers")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    [SwaggerOperation(Summary = "Gets all users with Sewer role")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(List<SewerDto>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "User was not authorized")]
    public async Task<IActionResult> GetSewers()
    {
        List<SewerDto> sewers = await _mediator.Send(new GetSewersQuery());
        return Ok(sewers);
    }

    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Registers a new user",
        Description = "'role' property can be ignored, by default all users have 'User' role\n\n" +
        "clientURIForEmailConfirmation - the base URI to the page where email confirmation performs, e.g. http://localhost:4200/emailConfirmation")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Provided username has already been taken. See details in the error response")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Provided passwords do not match. See details in the error response")]
    [SwaggerResponse((int)HttpStatusCode.UnprocessableEntity, Description = "Other validation errors. See details in the error response")]
    public async Task<IActionResult> Post([FromBody] CreateUserCommand createUserCommand)
    {
        createUserCommand.Role = "User";

        try
        {
            return Ok(await _mediator.Send(createUserCommand));
        }
        catch (UsernameAlreadyTakenException)
        {
            return Conflict(AddModelStateError("username", ErrorMessagesConstants.USERNAME_ALREADY_TAKEN));
        }
        catch (PasswordsMismatchException)
        {
            return BadRequest(AddModelStateError("password", ErrorMessagesConstants.PASSWORDS_DO_NOT_MATCH));
        }
        catch (IdentityResultException ex)
        {
            return UnprocessableEntity(AddModelStateError("model", ex.Message));
        }
    }

    [HttpPost("confirmEmail")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Performs confirmation of the email based on the given token",
        Description = "In the 'Development' mode validation of the email is disabled")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.UnprocessableEntity, Description = "Token validation errors, e.g. invalid token provided. See details in the error response")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "User with provided email was not found")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand confirmEmailCommand)
    {
        bool isSucceeded;
        try
        {
            isSucceeded = await _mediator.Send(confirmEmailCommand);
        }
        catch (IdentityResultException ex)
        {
            return UnprocessableEntity(AddModelStateError("model", ex.Message));
        }

        if (!isSucceeded)
            return NotFound("User with given email was not found");
        return Ok(isSucceeded);
    }

    [HttpGet("manageable")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    [SwaggerOperation(Summary = "Gets users available for activation/deactivation",
        Description = "Admin receives all users except Admins. Owner receives all users except Admins and Owners.")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ManageableUserDto>))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "User was not authorized")]
    public async Task<IActionResult> GetManageable()
    {
        string callerRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        IEnumerable<ManageableUserDto> users = await _mediator.Send(new GetUsersForActivationQuery { CallerRole = callerRole });
        return Ok(users);
    }

    [HttpPut("{id}/activation")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    [SwaggerOperation(Summary = "Activates or deactivates a user")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "User with provided Id was not found")]
    [SwaggerResponse((int)HttpStatusCode.UnprocessableEntity, Description = "Errors occurred during activation status change. See details in the error response")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "User was not authorized")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, Description = "User is not administrator")]
    public async Task<IActionResult> SetActivationStatus(Guid id, [FromBody] bool isActive)
    {
        bool isSucceeded;
        try
        {
            isSucceeded = await _mediator.Send(new SetUserActivationStatusCommand { UserId = id, IsActive = isActive });
        }
        catch (IdentityResultException ex)
        {
            return UnprocessableEntity(AddModelStateError("model", ex.Message));
        }

        if (!isSucceeded)
            return NotFound("User with given Id was not found");
        return Ok(isSucceeded);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    [SwaggerOperation(Summary = "Deletes app user by Id",
        Description = "Available only for administrators")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.UnprocessableEntity, Description = "Errors occurred during user deletion. See details in the error response")]
    [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "User with provided Id was not found")]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "User was not authorized")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, Description = "User is not administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool isSucceeded;
        try
        {
            isSucceeded = await _mediator.Send(new DeleteUserCommand { Id = id });
        }
        catch (IdentityResultException ex)
        {
            return UnprocessableEntity(AddModelStateError("model", ex.Message));
        }

        if (!isSucceeded)
            return NotFound("User with given Id was not found");
        return Ok(isSucceeded);
    }

    private static ModelStateDictionary AddModelStateError(string field, string error)
    {
        ModelStateDictionary modelState = new();
        modelState.TryAddModelError(field, error);
        return modelState;
    }
}
