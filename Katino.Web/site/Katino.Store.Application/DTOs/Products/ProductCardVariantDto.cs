namespace Katino.Store.Application.DTOs.Products;

public class ProductCardVariantDto
{
    public Guid Id { get; set; }
    public ProductCardSizeDto Size { get; set; }
    public ProductCardColorDto Color { get; set; }
    public ProductVariantStatusDto Status { get; set; }
    public string Article { get; set; }
    public List<ProductCardPhotoDto> Photos { get; set; }
    public List<ProductCardMeasurementDto> Measurements { get; set; }
}
