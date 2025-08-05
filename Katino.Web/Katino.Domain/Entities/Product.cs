namespace Katino.Domain.Entities;

// TODO set length in ProductVariant (in ProductVariantMeasurement)
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Article { get; set; }
    public Guid ColorId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal CostPrice { get; set; } // Себестоимость
    public decimal DropPrice { get; set; } // Дроп цена
    public decimal Price { get; set; } // Цена
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Color Color { get; set; }
    public Category Category { get; set; }
    public List<ProductPhoto> Photos { get; set; } = [];
    public List<ProductVariant> Variants { get; set; } = [];
}
