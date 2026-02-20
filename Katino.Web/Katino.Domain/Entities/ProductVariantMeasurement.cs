namespace Katino.Domain.Entities;

public class ProductVariantMeasurement
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid MeasurementTypeId { get; set; }
    public string Value { get; set; }

    // Navigation properties
    public ProductVariant ProductVariant { get; set; }
    public MeasurementType MeasurementType { get; set; }
}

