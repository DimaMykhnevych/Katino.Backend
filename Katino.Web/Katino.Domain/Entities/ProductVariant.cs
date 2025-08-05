using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid SizeId { get; set; }
    public ProductStatus Status { get; set; }
    public int QuantityInStock { get; set; }
    public int QuantityDropSold { get; set; }
    public int QuantityRegularSold { get; set; }
    public bool IsDrop { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    // Navigation properties
    public Product Product { get; set; }
    public Size Size { get; set; }
    public List<ProductVariantMeasurement> Measurements { get; set; } = [];


    // Calculated properties
    public int TotalSold => QuantityDropSold + QuantityRegularSold;
    public int AvailableQuantity => QuantityInStock - TotalSold;
}

