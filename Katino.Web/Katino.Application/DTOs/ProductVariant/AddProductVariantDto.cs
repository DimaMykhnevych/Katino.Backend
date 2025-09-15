using Katino.Application.DTOs.ProductVariantMeasurement;

namespace Katino.Application.DTOs.ProductVariant;

public class AddProductVariantDto
{
    public Guid ProductId { get; set; }
    public Guid SizeId { get; set; }
    public ProductStatusDto Status { get; set; }
    public Guid ColorId { get; set; }
    public int QuantityInStock { get; set; }
    public int QuantityDropSold { get; set; }
    public int QuantityRegularSold { get; set; }
    public bool IsDrop { get; set; }
    public string Article { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<AddProductVariantMeasurementDto> Measurements { get; set; } = [];
}
