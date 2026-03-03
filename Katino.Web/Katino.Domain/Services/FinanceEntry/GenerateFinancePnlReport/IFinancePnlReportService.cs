using Katino.Domain.Models.Pnl;

namespace Katino.Domain.Services.FinanceEntryN.GenerateFinancePnlReport;

public interface IFinancePnlReportService
{
    Task<PnlReport> BuildAsync(int year, CancellationToken ct);
}
