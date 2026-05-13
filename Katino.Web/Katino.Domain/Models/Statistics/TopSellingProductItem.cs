namespace Katino.Domain.Models.Statistics;

public class TopSellingProductItem
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public int TotalSold { get; set; }
}
