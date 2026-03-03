using AutoMapper;
using Katino.Application.DTOs.Pnl;
using Katino.Domain.Services.FinanceEntryN.GenerateFinancePnlReport;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.FinanceEntryN.GetPnlReport;

public class GetPnlReportQueryHandler : IRequestHandler<GetPnlReportQuery, PnlReportDto>
{
    private readonly IFinancePnlReportService _service;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetPnlReportQueryHandler(
        IFinancePnlReportService service,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _service = service;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(GetPnlReportQueryHandler));
    }

    public async Task<PnlReportDto> Handle(GetPnlReportQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get PnL report query");
        ArgumentNullException.ThrowIfNull(request);

        var year = request.Year ?? DateTime.UtcNow.Year;
        var report = await _service.BuildAsync(year, cancellationToken);
        return _mapper.Map<PnlReportDto>(report);
    }
}
