namespace Katino.Application.DTOs.Statistics;

public class SewingStatisticsItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public Guid SewerId { get; set; }
    public string SewerName { get; set; }
    public int TotalSewed { get; set; }
}
