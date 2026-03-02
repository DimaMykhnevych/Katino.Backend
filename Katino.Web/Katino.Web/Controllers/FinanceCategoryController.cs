using Katino.Application.Commands.FinanceCategoryN.AddFinanceCategory;
using Katino.Application.Commands.FinanceCategoryN.HideFinanceCategory;
using Katino.Application.DTOs.FinanceCategory;
using Katino.Application.Queries.FinanceCategoryN.GetFinanceCategories;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = Role.Admin)]
[ApiController]
public class FinanceCategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinanceCategoryDto>>> Get([FromQuery] GetFinanceCategoriesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<FinanceCategoryDto>> Create([FromBody] AddFinanceCategoryCommand request)
    {
        var category = await _mediator.Send(request);
        return Ok(category);
    }

    [HttpPost("{id:guid}/hide")]
    public async Task<ActionResult<bool>> Hide(Guid id)
    {
        var ok = await _mediator.Send(new HideFinanceCategoryCommand { Id = id });
        return Ok(ok);
    }
}
