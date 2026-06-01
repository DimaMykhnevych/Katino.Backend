using Katino.Application.Commands.CollectionN.AddCollection;
using Katino.Application.Commands.CollectionN.DeleteCollection;
using Katino.Application.Commands.CollectionN.UpdateCollectionProducts;
using Katino.Application.DTOs.Collection;
using Katino.Application.Queries.CollectionN.GetCollections;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

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
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Get()
    {
        List<CollectionDto> collections = await _mediator.Send(new GetCollectionsQuery());
        return Ok(collections);
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Add([FromBody] AddCollectionCommand addCollectionCommand)
    {
        CollectionDto result = await _mediator.Send(addCollectionCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpPut("{collectionId}/products")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> UpdateProducts(Guid collectionId, [FromBody] List<Guid> productIds)
    {
        UpdateCollectionProductsCommand command = new()
        {
            CollectionId = collectionId,
            ProductIds = productIds
        };
        CollectionDto result = await _mediator.Send(command);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpDelete("{collectionId}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Delete(Guid collectionId)
    {
        DeleteCollectionCommand command = new() { Id = collectionId };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
