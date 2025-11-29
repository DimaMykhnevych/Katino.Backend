using Katino.Application.Commands.CrmUserSettingsN.DeleteCrmUserSettings;
using Katino.Application.Commands.CrmUserSettingsN.AddCrmUserSettings;
using Katino.Application.Commands.CrmUserSettingsN.UpdateCrmUserSettings;
using Katino.Application.Queries.CrmUserSettings.GetCrmUserSettings;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CrmUserSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrmUserSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Get()
    {
        var user = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID).Value;
        var query = new GetCrmUserSettingsQuery() {AppUserId = Guid.Parse(user) };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromBody] AddCrmUserSettingsCommand crmUserSettingsCommand)
    {
        var user = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID).Value;
        crmUserSettingsCommand.AppUserId = Guid.Parse(user);
        var result = await _mediator.Send(crmUserSettingsCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Update([FromBody] UpdateCrmUserSettingsCommand crmUserSettingsCommand)
    {
        var user = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID).Value;
        crmUserSettingsCommand.AppUserId = Guid.Parse(user);
        var result = await _mediator.Send(crmUserSettingsCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpDelete("{crmUserSettingsId}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(Guid crmUserSettingsId)
    {
        var user = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID).Value;
        DeleteCrmUserSettingsCommand command = new() { Id = crmUserSettingsId, AppUserId = Guid.Parse(user) };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
