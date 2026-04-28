using Katino.Application.Commands.OrderTag.DetachOrderTag;
using Katino.Application.Queries.OrderTag.GetOrderTags;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderTagController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderTagController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> GetAll([FromQuery] string search = null, [FromQuery] bool? customOnly = null)
    {
        var result = await _mediator.Send(new GetOrderTagsQuery { Search = search, CustomOnly = customOnly });
        return Ok(result);
    }

    [HttpDelete("{orderId}/{tagId}")]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> Detach(Guid orderId, Guid tagId)
    {
        DetachOrderTagCommand command = new() { OrderId = orderId, TagId = tagId };
        bool result = await _mediator.Send(command);
        return result ? Ok() : BadRequest();
    }
}
