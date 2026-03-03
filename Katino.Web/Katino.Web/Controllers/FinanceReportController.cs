using Katino.Application.DTOs.Pnl;
using Katino.Application.Queries.FinanceEntryN.GetPnlReport;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
[ApiController]
public class FinanceReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("pnl")]
    public async Task<ActionResult<PnlReportDto>> GetPnl([FromQuery] int? year, CancellationToken ct)
    {
        var res = await _mediator.Send(new GetPnlReportQuery { Year = year }, ct);
        return Ok(res);
    }
}
