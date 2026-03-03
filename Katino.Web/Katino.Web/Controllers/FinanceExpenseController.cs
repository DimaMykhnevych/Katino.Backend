using Katino.Application.Commands.FinanceEntryN.CreateManualExpense;
using Katino.Application.Commands.FinanceEntryN.DeleteManualExpense;
using Katino.Application.Commands.FinanceEntryN.UpdateManualExpense;
using Katino.Application.DTOs.FinanceEntry;
using Katino.Application.Queries.FinanceEntryN.GetManualExpenses;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
[ApiController]
public class FinanceExpenseController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceExpenseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<FinanceExpenseDto>>> Get([FromQuery] int? year, CancellationToken ct)
    {
        var res = await _mediator.Send(new GetManualExpensesQuery
        {
            Year = year,
        }, ct);
        return Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateManualExpenseCommand cmd, CancellationToken ct)
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == AuthorizationConstants.ID).Value;
        cmd.CreatedBy = Guid.Parse(userIdString);

        var result = await _mediator.Send(cmd, ct);
        return result ? Ok(result) : BadRequest();
    }

    [HttpPut]
    public async Task<ActionResult<bool>> Update([FromBody] UpdateManualExpenseCommand cmd, CancellationToken ct)
    {
        var result = await _mediator.Send(cmd, ct);
        return result ? Ok(result) : BadRequest();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<bool>> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteManualExpenseCommand { Id = id }, ct);
        return result ? Ok(result) : BadRequest();
    }
}
