using Katino.Application.DTOs.MeasurementType;
using MediatR;

namespace Katino.Application.Queries.MeasurementTypeN.GetMeasurementTypes;

public class GetMeasurementTypesQuery : IRequest<GetMeasurementTypeDto>
{
    public string Name { get; set; }
}
