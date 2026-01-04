using Katino.Application.DTOs.Color;
using Katino.Application.DTOs.Product;
using Katino.Application.DTOs.Size;

namespace Katino.Application.DTOs.ProductVariant;

public class ProductVariantForOrderDto
{
    public Guid Id { get; set; }
    public ProductStatusDto Status { get; set; }
    public string Article { get; set; }

    public ColorDto Color { get; set; }
    public ProductForOrderDto Product { get; set; }
    public SizeDto Size { get; set; }
}
