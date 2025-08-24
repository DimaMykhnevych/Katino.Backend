using Katino.Application.DTOs.MeasurementType;

namespace Katino.Application.DTOs.ProductVariantMeasurement;

public class GetProductVariantMeasurementDto
{
    public Guid Id { get; set; }
    public Guid MeasurementTypeId { get; set; }
    public decimal Value { get; set; }
    public MeasurementTypeDto MeasurementType { get; set; }
}
