namespace Katino.Store.Application.DTOs.Products;

public class GetProductsDto
{
    public List<ProductListItemDto> Items { get; set; }
    public int TotalCount { get; set; }
}
