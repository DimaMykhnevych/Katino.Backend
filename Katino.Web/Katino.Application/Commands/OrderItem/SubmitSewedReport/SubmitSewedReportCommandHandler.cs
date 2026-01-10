using AutoMapper;
using Katino.Domain.Models;
using Katino.Domain.Services.OrderItemN.SewingProductionReportService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.OrderItemN.SubmitSewedReport;

public class SubmitSewedReportCommandHandler : IRequestHandler<SubmitSewedReportCommand, bool>
{
    private readonly ISewingProductionReportService _sewingProductionReportService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public SubmitSewedReportCommandHandler(
        ISewingProductionReportService sewingProductionReportService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _sewingProductionReportService = sewingProductionReportService;
        _logger = loggerFactory?.CreateLogger(nameof(SubmitSewedReportCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(SubmitSewedReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling submit sewed report request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            SewedReport report = _mapper.Map<SewedReport>(request);
             await _sewingProductionReportService.ApplySewedAsync(report, request.SubmittedBy).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during submiting sewed report");
            return false;
        }
    }
}
