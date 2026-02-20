using Katino.Application.DTOs.Category;

namespace Katino.Application.DTOs.Product;

public class ProductForOrderDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public CategoryDto Category { get; set; }
    public decimal CostPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal DropPrice { get; set; }
    public decimal Price { get; set; }
}
