namespace Katino.Application.DTOs.Statistics;

public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public int TotalSold { get; set; }
}
