using Katino.Application.DTOs.Statistics;
using MediatR;

namespace Katino.Application.Queries.Statistics.GetSewingStatistics;

public class GetSewingStatisticsQuery : IRequest<GetSewingStatisticsDto>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}
