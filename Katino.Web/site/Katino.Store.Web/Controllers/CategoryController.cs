using Katino.Store.Application.DTOs.Categories;
using Katino.Store.Application.Queries.Categories.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Katino.Store.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Gets all categories with product counts. Only categories with at least one available product are returned.")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<CategoryListItemDto>))]
    public async Task<IActionResult> GetCategories()
    {
        IEnumerable<CategoryListItemDto> result = await _mediator.Send(new GetCategoriesQuery());
        return Ok(result);
    }
}
