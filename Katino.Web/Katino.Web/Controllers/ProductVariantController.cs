using Katino.Application.Commands.ProductVariantN.AddProductVariant;
using Katino.Application.Commands.ProductVariantN.DeleteProductVariant;
using Katino.Application.Commands.ProductVariantN.UpdateProductVariant;
using Katino.Application.DTOs.ProductVariant;
using Katino.Application.Queries.Article.GetGeneratedArticle;
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
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager}")]
    public async Task<IActionResult> Get([FromQuery] GetProductVariantsQuery getProductVariantsQuery)
    {
        GetProductVariantDto productVariants = await _mediator.Send(getProductVariantsQuery);
        return Ok(productVariants);
    }

    [HttpGet("article/generate")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetGeneratedArticle()
    {
        string article = await _mediator.Send(new GetGeneratedArticleQuery());
        return Ok(article);
    }


    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromForm] AddProductVariantCommand addProductVariantCommand)
    {
        bool result = await _mediator.Send(addProductVariantCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Update([FromForm] UpdateProductVariantCommand updateProductVariantCommand)
    {
        bool result = await _mediator.Send(updateProductVariantCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpDelete("{productVariantId}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(Guid productVariantId)
    {
        DeleteProductVariantCommand command = new() { Id = productVariantId };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
