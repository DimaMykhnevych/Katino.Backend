using Katino.Application.DTOs.Category;

namespace Katino.Application.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Article { get; set; }
    public decimal CostPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal DropPrice { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CategoryDto Category { get; set; }
}
