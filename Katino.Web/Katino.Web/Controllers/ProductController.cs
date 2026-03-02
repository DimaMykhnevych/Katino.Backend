using Katino.Application.Commands.ProductN.AddProduct;
using Katino.Application.Commands.ProductN.DeleteProduct;
using Katino.Application.Commands.ProductN.UpdateProduct;
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
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Get([FromQuery] GetProductsQuery getProductsQuery)
    {
        GetProductDto products = await _mediator.Send(getProductsQuery);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Add([FromBody] AddProductCommand addProductCommand)
    {
        ProductDto result = await _mediator.Send(addProductCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Update([FromBody] UpdateProductCommand updateProductCommand)
    {
        ProductDto result = await _mediator.Send(updateProductCommand);
        return result != null ? Ok(result) : BadRequest();
    }

    [HttpDelete("{productId}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Delete(Guid productId)
    {
        DeleteProductCommand command = new() { Id = productId };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
