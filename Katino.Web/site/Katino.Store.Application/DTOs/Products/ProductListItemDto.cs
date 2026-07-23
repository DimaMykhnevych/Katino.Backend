namespace Katino.Store.Application.DTOs.Products;

public class ProductListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public bool HasDiscount { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string PhotoUrl { get; set; }
    public List<ProductColorDto> Colors { get; set; }
}
