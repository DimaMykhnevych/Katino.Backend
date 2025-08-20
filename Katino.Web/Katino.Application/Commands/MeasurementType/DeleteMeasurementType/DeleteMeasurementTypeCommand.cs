using MediatR;

namespace Katino.Application.Commands.MeasurementTypeN.DeleteMeasurementType;

public class DeleteMeasurementTypeCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
