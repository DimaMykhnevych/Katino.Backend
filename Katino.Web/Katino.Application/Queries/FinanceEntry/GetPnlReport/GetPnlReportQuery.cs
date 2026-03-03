using Katino.Application.DTOs.Pnl;
using MediatR;

namespace Katino.Application.Queries.FinanceEntryN.GetPnlReport;

public class GetPnlReportQuery : IRequest<PnlReportDto>
{
    public int? Year { get; set; }
}
