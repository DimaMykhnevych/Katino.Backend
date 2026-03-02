using Katino.Application.Commands.MeasurementTypeN.AddMeasurementType;
using Katino.Application.Commands.MeasurementTypeN.DeleteMeasurementType;
using Katino.Application.Commands.MeasurementTypeN.UpdateMeasurementType;
using Katino.Application.DTOs.MeasurementType;
using Katino.Application.Queries.MeasurementTypeN.GetMeasurementTypes;
using Katino.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Katino.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeasurementTypeController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeasurementTypeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetMeasurementTypesQuery getMeasurementTypesQuery)
    {
        GetMeasurementTypeDto measurementTypes = await _mediator.Send(getMeasurementTypesQuery);
        return Ok(measurementTypes);
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Add([FromBody] AddMeasurementTypeCommand addMeasurementTypeCommand)
    {
        bool result = await _mediator.Send(addMeasurementTypeCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpPut]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Update([FromBody] UpdateMeasurementTypeCommand updateMeasurementTypeCommand)
    {
        bool result = await _mediator.Send(updateMeasurementTypeCommand);
        return result ? Ok(result) : BadRequest();
    }

    [HttpDelete("{measurementTypeId}")]
    [Authorize(Roles = $"{Role.Admin},{Role.Owner}")]
    public async Task<IActionResult> Delete(Guid measurementTypeId)
    {
        DeleteMeasurementTypeCommand command = new() { Id = measurementTypeId };
        bool result = await _mediator.Send(command);
        return result ? Ok(result) : BadRequest();
    }
}
