namespace Katino.Domain.Models.Statistics;

public class SewingStatisticsItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public Guid SewerId { get; set; }
    public string SewerName { get; set; }
    public int TotalSewed { get; set; }
}
