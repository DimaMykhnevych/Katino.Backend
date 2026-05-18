using AutoMapper;
using Katino.Domain.Models;
using Katino.Domain.Services.OrderItemN.SewingProductionReportService;
using Katino.Domain.Services.TelegramN;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.OrderItemN.SubmitSewedReport;

public class SubmitSewedReportCommandHandler : IRequestHandler<SubmitSewedReportCommand, bool>
{
    private readonly ISewingProductionReportService _sewingProductionReportService;
    private readonly ITelegramService _telegramService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public SubmitSewedReportCommandHandler(
        ISewingProductionReportService sewingProductionReportService,
        ITelegramService telegramService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _sewingProductionReportService = sewingProductionReportService;
        _telegramService = telegramService;
        _logger = loggerFactory?.CreateLogger(nameof(SubmitSewedReportCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(SubmitSewedReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling submit sewed report request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var report = _mapper.Map<List<SewedReport>>(request.ReportItems);
            await _sewingProductionReportService.ApplySewedAsync(report, request.ReportItems.First().SubmittedBy).ConfigureAwait(false);

            if (request.IsSewer)
            {
                try
                {
                    await _telegramService.SendSewingReportNotificationAsync(report, request.SubmitterName)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured when sending sewing report notification");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during submiting sewed report");
            return false;
        }
    }
}
