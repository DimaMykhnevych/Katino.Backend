namespace Katino.Application.DTOs.Product;

public class GetProductDto
{
    public IEnumerable<ProductDto> Products { get; set; }
    public int ResultsAmount { get; set; }
}
