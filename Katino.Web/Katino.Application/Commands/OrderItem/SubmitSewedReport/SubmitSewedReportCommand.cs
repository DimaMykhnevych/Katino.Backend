using Katino.Application.DTOs.OrderItem;
using MediatR;

namespace Katino.Application.Commands.OrderItemN.SubmitSewedReport;

public class SubmitSewedReportCommand : IRequest<bool>
{
    public List<SubmitSewedReportItemDto> ReportItems { get; set; }
}
