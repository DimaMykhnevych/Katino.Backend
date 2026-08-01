using Katino.Store.Application.DTOs.Collections;
using Katino.Store.Application.Queries.Collections.GetCollections;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Katino.Store.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CollectionController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollectionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Gets all collections with product counts. Only collections with at least one available product are returned.")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<CollectionListItemDto>))]
    public async Task<IActionResult> GetCollections()
    {
        IEnumerable<CollectionListItemDto> result = await _mediator.Send(new GetCollectionsQuery());
        return Ok(result);
    }
}
