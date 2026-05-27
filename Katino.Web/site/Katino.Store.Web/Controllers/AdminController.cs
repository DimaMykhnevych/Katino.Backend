using Katino.Domain.Constants;
using Katino.Store.Application.DTOs;
using Katino.Store.Application.Queries.AppLogs.GetLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Katino.Store.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = Role.Admin)]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("logs-plain-text")]
    [SwaggerOperation(Summary = "Gets logs for given date in text representation")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LogsDto))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "User was not authorized")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, Description = "User is not administrator")]
    public async Task<IActionResult> GetLogs([FromQuery] DateTime date)
    {
        LogsDto logs = await _mediator.Send(new GetLogsQuery { Date = date, GetLogsMode = GetLogsModeDto.PlainText });
        return Ok(logs);
    }

    [HttpGet("logs-file")]
    [SwaggerOperation(Summary = "Gets logs for given date in file representation")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Stream))]
    [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "User was not authorized")]
    [SwaggerResponse((int)HttpStatusCode.Forbidden, Description = "User is not administrator")]
    public async Task<IActionResult> GetFileLogs([FromQuery] DateTime date)
    {
        LogsDto logs = await _mediator.Send(new GetLogsQuery { Date = date, GetLogsMode = GetLogsModeDto.File });
        return File(logs.LogsStream, "application/octet-stream", logs.FileName);
    }
}
