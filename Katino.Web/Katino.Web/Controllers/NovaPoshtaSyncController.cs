using Katino.Application.Commands.NovaPost.TriggerSync;
using Katino.Application.DTOs.NovaPost;
using Katino.Application.Queries.NovaPost.GetCurrentSyncStatus;
using Katino.Application.Queries.NovaPost.GetSyncHistory;
using Katino.Domain.Constants;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NovaPoshtaSyncController : ControllerBase
{
    private readonly IMediator _mediator;

    public NovaPoshtaSyncController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("status")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> GetSyncStatus()
    {
        GetCurrentSyncStatusDto result = await _mediator.Send(new GetCurrentSyncStatusQuery() { SyncType = SyncTypeDto.Warehouses });
        return Ok(result);
    }

    [HttpPost("trigger")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> TriggerSync()
    {
        var user = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID).Value;
        bool result = await _mediator.Send(new TriggerSyncCommand() { SyncType = SyncTypeDto.Warehouses, TriggeredBy = Guid.Parse(user) });
        return result ? Accepted() : BadRequest();
    }

    [HttpGet("history")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<ActionResult<List<NovaPoshtaSyncStatus>>> GetSyncHistory([FromQuery] int limit = 20)
    {
        GetSyncRecordDto history = await _mediator.Send(new GetSyncHistoryQuery() { Limit = limit, SyncType = SyncTypeDto.Warehouses });
        return Ok(history);
    }
}
