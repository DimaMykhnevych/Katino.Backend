using Katino.Domain.Constants;
using Katino.Domain.Exceptions;
using Katino.Store.Application.Commands.Auth.ConfirmCustomerEmail;
using Katino.Store.Application.Commands.Auth.RegisterCustomer;
using Katino.Store.Application.Commands.Auth.SignInCustomer;
using Katino.Store.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Katino.Store.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerAuthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [SwaggerOperation(Summary = "Registers a new customer account",
        Description = "Sends a confirmation email; the account cannot log in until confirmed.\n\n" +
        "clientUriForEmailConfirmation - the base URI to the page where email confirmation performs, e.g. https://katino.store/emailConfirmation\n\n" +
        "Password must be at least 7 characters long and contain an uppercase letter, a lowercase letter, a digit, " +
        "and a special character (one of !@#$%^&*()_+-=[]{}|;:'\",.<>/?).\n\n" +
        "Errors are returned as ASP.NET validation problem details (`errors.<field>[]`). The error value is always a " +
        "stable machine-readable code, not display text - switch on it directly instead of the error message:\n\n" +
        "- errors.email = [\"emailRequired\"] (400)\n" +
        "- errors.email = [\"emailInvalidFormat\"] (400)\n" +
        "- errors.email = [\"emailAlreadyTaken\"] (409)\n" +
        "- errors.password = [\"passwordRequired\"] (400)\n" +
        "- errors.confirmPassword = [\"confirmPasswordRequired\"] (400)\n" +
        "- errors.password = [\"passwordMismatch\"] (400) - password/confirmPassword don't match\n" +
        "- errors.password = [\"passwordTooWeak\"] (400) - doesn't meet the strength requirement above")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.Conflict, Description = "Provided email has already been taken. errors.email = [\"emailAlreadyTaken\"]")]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Provided passwords do not match, the password is too weak, or built-in model validation failed. See Description above for the exact codes")]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
        }
        catch (EmailAlreadyTakenException)
        {
            return ValidationProblem(statusCode: (int)HttpStatusCode.Conflict, modelStateDictionary: AddModelStateError("email", ErrorMessagesConstants.EMAIL_ALREADY_TAKEN));
        }
        catch (PasswordsMismatchException)
        {
            return ValidationProblem(modelStateDictionary: AddModelStateError("password", ErrorMessagesConstants.PASSWORD_MISMATCH));
        }
        catch (WeakPasswordException)
        {
            return ValidationProblem(modelStateDictionary: AddModelStateError("password", ErrorMessagesConstants.PASSWORD_TOO_WEAK));
        }
    }

    [HttpPost]
    [Route("confirm-email")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [SwaggerOperation(Summary = "Confirms a customer's email based on the token sent during registration")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(bool))]
    [SwaggerResponse((int)HttpStatusCode.UnprocessableEntity, Description = "Token is invalid, already used, or expired. See details in the error response")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmCustomerEmailCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
        }
        catch (EmailConfirmationTokenInvalidException)
        {
            return ValidationProblem(statusCode: (int)HttpStatusCode.UnprocessableEntity, modelStateDictionary: AddModelStateError("token", ErrorMessagesConstants.EMAIL_CONFIRMATION_TOKEN_INVALID));
        }
    }

    [HttpPost]
    [Route("token")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [SwaggerOperation(Summary = "Gets a Bearer token with basic customer info in case of successful authorization",
        Description = "As a result returns the model with LoginErrorCode property.\n\nPossible LoginErrorCode values:\n\n" +
        "0 = Invalid email or password\n\n" +
        "1 = Email confirmation required\n\n" +
        "100 = Customer was successfully authorized")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CustomerAuthResultDto))]
    public async Task<IActionResult> Login([FromBody] SignInCustomerCommand command)
    {
        CustomerAuthResultDto result = await _mediator.Send(command);
        return Ok(result);
    }

    private static ModelStateDictionary AddModelStateError(string field, string error)
    {
        ModelStateDictionary modelState = new();
        modelState.TryAddModelError(field, error);
        return modelState;
    }
}
