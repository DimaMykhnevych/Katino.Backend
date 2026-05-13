namespace Katino.Domain.Models.Statistics;

public class TopSellingProductsResult
{
    public IEnumerable<TopSellingProductItem> Products { get; set; }
    public int TotalCount { get; set; }
}
