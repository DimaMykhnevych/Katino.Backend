namespace Katino.Domain.Models.Statistics;

public class SewingStatisticsResult
{
    public IEnumerable<SewingStatisticsItem> Items { get; set; }
    public int TotalCount { get; set; }
}
