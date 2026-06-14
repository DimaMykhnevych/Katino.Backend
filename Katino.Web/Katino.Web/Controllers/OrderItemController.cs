using Katino.Application.Commands.OrderItemN.SubmitSewedReport;
using Katino.Application.Queries.OrderItemN.GetGroupedSewingQueue;
using Katino.Application.Queries.OrderItemN.GetSewingQueue;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderItemController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("sewing-queue")]
    [Authorize(Roles = $"{Role.Admin},{Role.Sewer},{Role.Owner}")]
    public async Task<IActionResult> GetSewingQueue([FromQuery] Guid? sewerId)
    {
        var resolvedSewerId = ResolveSewerId(sewerId);
        var result = await _mediator.Send(new GetSewingQueueQuery { SewerId = resolvedSewerId });
        return Ok(result);
    }

    [HttpGet("sewing-queue-grouped")]
    [Authorize(Roles = $"{Role.Admin},{Role.Sewer},{Role.Owner}")]
    public async Task<IActionResult> GetSewingQueueGrouped([FromQuery] Guid? sewerId)
    {
        var resolvedSewerId = ResolveSewerId(sewerId);
        var result = await _mediator.Send(new GetGroupedSewingQueueQuery { SewerId = resolvedSewerId });
        return Ok(result);
    }

    [HttpPost("sewing-report")]
    [Authorize(Roles = $"{Role.Admin},{Role.Sewer},{Role.Owner}")]
    public async Task<IActionResult> SubmitSewingReport([FromBody] SubmitSewedReportCommand submitSewedReportCommand)
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID).Value;
        var userIdGuid = Guid.Parse(userIdString);
        foreach (var item in submitSewedReportCommand.ReportItems)
        {
            item.SubmittedBy = userIdGuid;
        }

        submitSewedReportCommand.IsSewer = User.IsInRole(Role.Sewer);
        submitSewedReportCommand.SubmitterName = User.Identity?.Name ?? "Невідомо";

        var result = await _mediator.Send(submitSewedReportCommand);
        return result ? Ok(result) : BadRequest();
    }

    private Guid? ResolveSewerId(Guid? requestedSewerId)
    {
        if (!User.IsInRole(Role.Sewer))
            return requestedSewerId;

        var userIdString = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID)?.Value;
        return userIdString != null ? Guid.Parse(userIdString) : null;
    }
}
