namespace Katino.Application.DTOs.MeasurementType;

public class GetMeasurementTypeDto
{
    public IEnumerable<MeasurementTypeDto> MeasurementTypes { get; set; }
    public int ResultsAmount { get; set; }
}
