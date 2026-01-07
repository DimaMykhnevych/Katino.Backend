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

    // TODO add role for sewer
    [HttpGet("sewing-queue")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetSewingQueue()
    {
        var result = await _mediator.Send(new GetSewingQueueQuery());
        return Ok(result);
    }
}
