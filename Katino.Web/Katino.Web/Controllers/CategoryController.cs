using Katino.Application.Commands.CategoryN.AddCategory;
using Katino.Application.Commands.CategoryN.DeleteCategory;
using Katino.Application.Commands.CategoryN.UpdateCategory;
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
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Add([FromBody] AddCategoryCommand addCategoryCommand)
    {
        CategoryDto result = await _mediator.Send(addCategoryCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Update([FromBody] UpdateCategoryCommand updateCategoryCommand)
    {
        CategoryDto result = await _mediator.Send(updateCategoryCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpDelete("{categoryId}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Delete(Guid categoryId)
    {
        DeleteCategoryCommand command = new() { Id = categoryId };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
