using AutoMapper;
using Katino.Application.DTOs.Statistics;
using Katino.Domain.Services.StatisticsN;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.Statistics.GetSewingStatistics;

public class GetSewingStatisticsQueryHandler : IRequestHandler<GetSewingStatisticsQuery, GetSewingStatisticsDto>
{
    private readonly ISewingStatisticsService _service;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetSewingStatisticsQueryHandler(
        ISewingStatisticsService service,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _service = service;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(GetSewingStatisticsQueryHandler));
    }

    public async Task<GetSewingStatisticsDto> Handle(GetSewingStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get sewing statistics");
        ArgumentNullException.ThrowIfNull(request);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var result = await _service.GetAsync(page, pageSize, request.From, request.To, cancellationToken);
        return _mapper.Map<GetSewingStatisticsDto>(result);
    }
}
