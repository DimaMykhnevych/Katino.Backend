namespace Katino.Application.DTOs.Statistics;

public class GetSewingStatisticsDto
{
    public IEnumerable<SewingStatisticsItemDto> Items { get; set; }
    public int ResultsAmount { get; set; }
}
