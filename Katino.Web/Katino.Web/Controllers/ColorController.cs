using Katino.Application.Commands.ColorN.AddColor;
using Katino.Application.Commands.ColorN.DeleteColor;
using Katino.Application.Commands.ColorN.UpdateColor;
using Katino.Application.DTOs.Color;
using Katino.Application.Queries.ColorN.GetColors;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ColorController : ControllerBase
{
    private readonly IMediator _mediator;

    public ColorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetColorsQuery getColorsQuery)
    {
        GetColorDto colors = await _mediator.Send(getColorsQuery);
        return Ok(colors);
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Add([FromBody] AddColorCommand addColorCommand)
    {
        ColorDto result = await _mediator.Send(addColorCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Update([FromBody] UpdateColorCommand updateColorCommand)
    {
        ColorDto result = await _mediator.Send(updateColorCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpDelete("{colorId}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Delete(Guid colorId)
    {
        DeleteColorCommand command = new() { Id = colorId };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
