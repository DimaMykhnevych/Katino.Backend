namespace Katino.Domain.Entities;

public class MeasurementType
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }

    // Navigation properties
    public List<ProductVariantMeasurement> Measurements { get; set; } = [];
}
