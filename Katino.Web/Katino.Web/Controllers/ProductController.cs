using Katino.Application.Commands.ProductN.AddProduct;
using Katino.Application.DTOs.Product;
using Katino.Application.Queries.ProductN.GetProducts;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

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
    public async Task<IActionResult> Get([FromQuery] GetProductsQuery getProductsQuery)
    {
        GetProductDto products = await _mediator.Send(getProductsQuery);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromBody] AddProductCommand addProductCommand)
    {
        bool result = await _mediator.Send(addProductCommand);
        return result ? Ok(result) : BadRequest();
    }
}
