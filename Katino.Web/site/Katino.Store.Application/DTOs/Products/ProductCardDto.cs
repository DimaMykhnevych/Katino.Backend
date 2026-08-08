namespace Katino.Store.Application.DTOs.Products;

public class ProductCardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ProductCardCategoryDto Category { get; set; }
    public decimal Price { get; set; }
    public bool HasDiscount { get; set; }
    public decimal? DiscountPrice { get; set; }
    public List<ProductCardVariantDto> Variants { get; set; }
}
