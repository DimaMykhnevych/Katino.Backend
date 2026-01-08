using Katino.Domain.Models;

namespace Katino.Domain.Services.OrderItemN.SewingProductionReportService;

public interface ISewingProductionReportService
{
    Task ApplySewedAsync(SewedReport report, CancellationToken ct = default);
}
