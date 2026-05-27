namespace Katino.Store.Application.DTOs.Products;

public class ProductListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public string PhotoUrl { get; set; }
}
