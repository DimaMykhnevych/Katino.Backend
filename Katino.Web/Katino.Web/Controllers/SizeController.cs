using Katino.Application.Commands.SizeN.AddSize;
using Katino.Application.Commands.SizeN.DeleteSize;
using Katino.Application.Commands.SizeN.UpdateSize;
using Katino.Application.DTOs.Size;
using Katino.Application.Queries.SizeN.GetSizes;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SizeController : ControllerBase
{
    private readonly IMediator _mediator;

    public SizeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetSizesQuery getSizesQuery)
    {
        GetSizeDto sizes = await _mediator.Send(getSizesQuery);
        return Ok(sizes);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromBody] AddSizeCommand addSizeCommand)
    {
        bool result = await _mediator.Send(addSizeCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Update([FromBody] UpdateSizeCommand updateSizeCommand)
    {
        bool result = await _mediator.Send(updateSizeCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpDelete("{sizeId}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(Guid sizeId)
    {
        DeleteSizeCommand command = new() { Id = sizeId };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
