using MediatR;

namespace Katino.Application.Commands.MeasurementTypeN.AddMeasurementType;

public class AddMeasurementTypeCommand : IRequest<bool>
{
    public string Name { get; set; }
    public string Unit { get; set; }
}
