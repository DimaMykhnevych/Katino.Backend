using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class ProductVariant : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid SizeId { get; set; }
    public ProductStatus Status { get; set; }
    public Guid ColorId { get; set; }
    public int QuantityInStock { get; set; }
    public int QuantityDropSold { get; set; }
    public int QuantityRegularSold { get; set; }
    public bool IsDrop { get; set; }
    public string Article { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public SewingQueueVisibility SewingQueueVisibility { get; set; } = SewingQueueVisibility.AllSewers;


    // Navigation properties
    public Color Color { get; set; }
    public Product Product { get; set; }
    public Size Size { get; set; }
    public List<ProductVariantMeasurement> Measurements { get; set; } = [];
    public List<ProductPhoto> Photos { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];
    public List<ProductVariantSewer> Sewers { get; set; } = [];


    // Calculated properties
    public int TotalSold => QuantityDropSold + QuantityRegularSold;
    public int AvailableQuantity => QuantityInStock - TotalSold;
}

