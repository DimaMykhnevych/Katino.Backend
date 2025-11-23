using Katino.Application.Queries.NpWarehouse.SearchNpWarehouses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NpWarehouseController : ControllerBase
{
    private readonly IMediator _mediator;

    public NpWarehouseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchNpWarehouses([FromQuery] SearchNpWarehousesQuery searchNpWarehousesQuery)
    {
        var warehouses = await _mediator.Send(searchNpWarehousesQuery);
        return Ok(warehouses);
    }

}
