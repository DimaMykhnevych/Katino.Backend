namespace Katino.Store.Application.DTOs.Products;

public class ProductCardMeasurementDto
{
    public Guid Id { get; set; }
    public string Value { get; set; }
    public ProductCardMeasurementTypeDto MeasurementType { get; set; }
}
