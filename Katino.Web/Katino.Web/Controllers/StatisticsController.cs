using Katino.Application.DTOs.Statistics;
using Katino.Application.Queries.Statistics.GetTopSellingProducts;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("top-selling-products")]
    public async Task<ActionResult<GetTopSellingProductsDto>> GetTopSellingProducts(
        [FromQuery] GetTopSellingProductsQuery query,
        CancellationToken ct)
    {
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
