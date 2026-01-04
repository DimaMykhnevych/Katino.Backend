using Katino.Application.DTOs.Category;

namespace Katino.Application.DTOs.Product;

public class ProductForOrderDto
{
    public string Name { get; set; }
    public CategoryDto Category { get; set; }
}
