using Katino.Store.Application.DTOs.Products;
using Katino.Store.Application.Queries.Products.GetProductCard;
using Katino.Store.Application.Queries.Products.GetProducts;
using Katino.Store.Application.Queries.Products.GetRecentProducts;
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

    [HttpGet("recent")]
    [SwaggerOperation(Summary = "Gets the 20 most recently added products. Response is cached for 10 minutes.")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<ProductListItemDto>))]
    public async Task<IActionResult> GetRecentProducts()
    {
        IEnumerable<ProductListItemDto> result = await _mediator.Send(new GetRecentProductsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Gets full product details for the product card, including all non-discontinued variants with photos and measurements.")]
    [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ProductCardDto))]
    [SwaggerResponse((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetProductCard(Guid id)
    {
        ProductCardDto result = await _mediator.Send(new GetProductCardQuery { Id = id });
        return result is null ? NotFound() : Ok(result);
    }
}
