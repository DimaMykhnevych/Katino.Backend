namespace Katino.Domain.Entities;

public class Color
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string HexCode { get; set; }

    // Navigation properties
    public List<ProductVariant> ProductVariants { get; set; } = [];
}
