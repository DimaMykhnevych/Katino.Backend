using Katino.Application.DTOs.Color;
using Katino.Application.DTOs.Product;
using Katino.Application.DTOs.ProductPhoto;
using Katino.Application.DTOs.ProductVariantMeasurement;
using Katino.Application.DTOs.Size;
using Katino.Application.DTOs.User;

namespace Katino.Application.DTOs.ProductVariant;

public class ProductVariantDto
{
    public Guid Id { get; set; }
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
    public SewingQueueVisibilityDto SewingQueueVisibility { get; set; } = SewingQueueVisibilityDto.AllSewers;
    public List<SewerDto> Sewers { get; set; } = [];

    public ColorDto Color { get; set; }
    public ProductDto Product { get; set; }
    public SizeDto Size { get; set; }
    public List<GetProductVariantMeasurementDto> Measurements { get; set; } = [];
    public List<ProductPhotoDto> Photos { get; set; } = [];
}
