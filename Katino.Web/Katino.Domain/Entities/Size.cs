using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class Size
{
    public Guid Id { get; set; }
    public string Name { get; set; } // XS, S, M, L, XL, 34, 35 etc
    public SizeType Type { get; set; } // Letter, Number


    // Navigation properties
    public List<ProductVariant> ProductVariants { get; set; } = [];
}
