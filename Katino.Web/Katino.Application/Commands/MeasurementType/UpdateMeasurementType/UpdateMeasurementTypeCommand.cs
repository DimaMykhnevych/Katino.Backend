using Katino.Application.DTOs.MeasurementType;
using MediatR;

namespace Katino.Application.Commands.MeasurementTypeN.UpdateMeasurementType;

public class UpdateMeasurementTypeCommand : IRequest<bool>
{
    public MeasurementTypeDto MeasurementType { get; set; }
}
