using Katino.Application.Commands.CategoryN.AddCategory;
using Katino.Application.DTOs.Category;
using Katino.Application.Queries.CategoryN.GetCategories;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

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
    public async Task<IActionResult> Get([FromQuery] GetCategoriesQuery getCategoriesQuery)
    {
        GetCategoryDto categories = await _mediator.Send(getCategoriesQuery);
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromBody] AddCategoryCommand addCategoryCommand)
    {
        bool result = await _mediator.Send(addCategoryCommand);
        return result ? Ok(result) : BadRequest();
    }
}
