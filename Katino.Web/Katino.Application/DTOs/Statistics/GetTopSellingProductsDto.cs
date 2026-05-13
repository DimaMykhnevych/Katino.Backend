namespace Katino.Application.DTOs.Statistics;

public class GetTopSellingProductsDto
{
    public IEnumerable<TopSellingProductDto> Products { get; set; }
    public int ResultsAmount { get; set; }
}
