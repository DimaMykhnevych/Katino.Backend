using Katino.Application.Commands.Telegram.SendTelegramTestMessage;
using Katino.Application.Commands.Telegram.UpdateTelegramSettings;
using Katino.Application.Queries.Telegram.GetTelegramChats;
using Katino.Application.Queries.Telegram.GetTelegramSettings;
using Katino.Application.Queries.Telegram.ValidateTelegramBot;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{Role.Owner},{Role.Admin}")]
public class TelegramController : ControllerBase
{
    private readonly IMediator _mediator;

    public TelegramController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _mediator.Send(new GetTelegramSettingsQuery());
        return Ok(result);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateTelegramSettingsCommand command)
    {
        var result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }

    [HttpGet("validate-bot")]
    public async Task<IActionResult> ValidateBot([FromQuery] string token)
    {
        try
        {
            var result = await _mediator.Send(new ValidateTelegramBotQuery { Token = token });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Bot token is invalid: {ex.Message}" });
        }
    }

    [HttpGet("chats")]
    public async Task<IActionResult> GetChats([FromQuery] string token)
    {
        try
        {
            var result = await _mediator.Send(new GetTelegramChatsQuery { Token = token });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Cannot retrieve chats: {ex.Message}" });
        }
    }

    [HttpPost("test-message")]
    public async Task<IActionResult> SendTestMessage()
    {
        try
        {
            await _mediator.Send(new SendTelegramTestMessageCommand());
            return Ok(true);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
