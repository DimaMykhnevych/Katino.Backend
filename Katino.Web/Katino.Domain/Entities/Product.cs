namespace Katino.Domain.Entities;

// TODO set length in ProductVariant (in ProductVariantMeasurement)
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CategoryId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal WholesalePrice { get; set;}
    public decimal DropPrice { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Category Category { get; set; }
    public List<ProductPhoto> Photos { get; set; } = [];
    public List<ProductVariant> Variants { get; set; } = [];
}
