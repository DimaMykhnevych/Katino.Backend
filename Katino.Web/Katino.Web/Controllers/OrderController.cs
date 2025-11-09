using Katino.Application.Commands.OrderN.AddOrder;
using Katino.Application.DTOs.Order;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    // TODO uncomment after testing
    //[Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromBody] AddOrderCommand addOrderCommand)
    {
        OrderCreationResultDto result = await _mediator.Send(addOrderCommand);
        return result.OrderAddedSuccessfully ? Ok(result) : BadRequest(result);
    }
}
