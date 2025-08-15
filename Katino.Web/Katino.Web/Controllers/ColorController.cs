using Katino.Application.DTOs.Color;
using Katino.Application.Queries.ColorN.GetColors;
using MediatR;
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
}
