using Katino.Application.Commands.DiscountN.AddDiscount;
using Katino.Application.Commands.DiscountN.DeleteDiscount;
using Katino.Application.Commands.DiscountN.SetDiscountActive;
using Katino.Application.Commands.DiscountN.UpdateDiscount;
using Katino.Application.DTOs.Discount;
using Katino.Application.Queries.DiscountN.GetDiscounts;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DiscountController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiscountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}, {Role.DirectManager}")]
    public async Task<IActionResult> Get()
    {
        List<DiscountDto> result = await _mediator.Send(new GetDiscountsQuery());
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Add([FromBody] AddDiscountCommand addDiscountCommand)
    {
        DiscountDto result = await _mediator.Send(addDiscountCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountCommand updateDiscountCommand)
    {
        updateDiscountCommand.Id = id;
        DiscountDto result = await _mediator.Send(updateDiscountCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        DeleteDiscountCommand command = new() { Id = id };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }

    [HttpPatch("{id}/active")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetDiscountActiveCommand command)
    {
        command.Id = id;
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
