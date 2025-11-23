using Katino.Application.DTOs.NovaPost;
using Katino.Application.Queries.NovaPost.GetNpCities;
using Katino.Application.Queries.NovaPost.GetNpSenderContactPersons;
using Katino.Domain.Constants;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NovaPostController : ControllerBase
{
    private readonly IMediator _mediator;

    public NovaPostController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("cities/search")]
    public async Task<ActionResult<List<NovaPoshtaSyncStatus>>> GetSyncHistory([FromQuery] string cityName)
    {
        GetNpCitiesResponseDto cities = await _mediator.Send(new GetNpCitiesQuery() { CityName = cityName });
        return Ok(cities);
    }

    [HttpGet("sender/contact-persons")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetSenderContactPersons()
    {
        var senderContactPersons = await _mediator.Send(new GetNpSenderContactPersonsQuery());
        return Ok(senderContactPersons);
    }
}
