using Katino.Store.Application.DTOs.Products;
using Katino.Store.Application.Queries.Products.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Katino.Store.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Gets paginated product list. Supports search by name or article.")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(GetProductsDto))]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        GetProductsDto result = await _mediator.Send(query);
        return Ok(result);
    }
}
