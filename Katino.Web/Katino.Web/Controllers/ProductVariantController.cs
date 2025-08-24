using Katino.Application.Commands.ProductVariantN.AddProductVariant;
using Katino.Application.DTOs.ProductVariant;
using Katino.Application.Queries.ProductVariantN.GetProductVariants;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductVariantController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductVariantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetProductVariantsQuery getProductVariantsQuery)
    {
        GetProductVariantDto productVariants = await _mediator.Send(getProductVariantsQuery);
        return Ok(productVariants);
    }


    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromBody] AddProductVariantCommand addProductVariantCommand)
    {
        bool result = await _mediator.Send(addProductVariantCommand);
        return result ? Ok(result) : BadRequest();
    }
}
