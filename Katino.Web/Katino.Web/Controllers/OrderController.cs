using Katino.Application.Commands.OrderN.AddOrder;
using Katino.Application.Commands.OrderN.DeleteOrder;
using Katino.Application.Commands.OrderN.SetOrderManualStatus;
using Katino.Application.Commands.OrderN.UpdateOrder;
using Katino.Application.DTOs.Order;
using Katino.Application.Queries.OrderN.GetNextOrderStatus;
using Katino.Application.Queries.OrderN.GetOrder;
using Katino.Application.Queries.OrderN.GetOrderById;
using Katino.Application.Queries.OrderN.PreviewOrderCost;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> Get([FromQuery] GetOrderQuery getOrderQuery)
    {
        var result = await _mediator.Send(getOrderQuery);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery { Id = id });
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> Add([FromBody] AddOrderCommand addOrderCommand)
    {
        OrderCreationResultDto result = await _mediator.Send(addOrderCommand);
        return result.OrderAddedSuccessfully ? Ok(result) : BadRequest(result);
    }

    [HttpPut]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> Update([FromBody] UpdateOrderCommand updateOrderCommand)
    {
        OrderUpdateResultDto result = await _mediator.Send(updateOrderCommand);
        return result.OrderUpdatedSuccessfully ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{orderId}")]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> Delete(Guid orderId)
    {
        DeleteOrderCommand command = new() { Id = orderId };
        var result = await _mediator.Send(command);
        return result.OrderDeletedSuccessfully ? Ok(result) : BadRequest(result);
    }

    [HttpGet("manual-status/get-next")]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> GetNextManualStatus([FromQuery] GetNextOrderStatusQuery getNextOrderStatusQuery)
    {
        OrderStatusDto[] result = await _mediator.Send(getNextOrderStatusQuery);
        return Ok(result);
    }

    [HttpPost("manual-status/set")]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> SetManualStatus([FromBody] SetOrderManualStatusCommand setOrderManualStatusCommand)
    {
        bool result = await _mediator.Send(setOrderManualStatusCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpPost("preview-cost")]
    [Authorize(Roles = $"{Role.Admin},{Role.DirectManager},{Role.Owner}")]
    public async Task<IActionResult> PreviewCost([FromBody] PreviewOrderCostQuery previewOrderCostQuery)
    {
        OrderPricingResultDto result = await _mediator.Send(previewOrderCostQuery);
        return Ok(result);
    }
}
