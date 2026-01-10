using MediatR;

namespace Katino.Application.Commands.OrderItemN.SubmitSewedReport;

public class SubmitSewedReportCommand : IRequest<bool>
{
    public Guid ProductVariantId { get; init; }
    public int ActualSewedQuantity { get; init; }
    public Guid? OrderItemId { get; init; }
    public Guid SubmittedBy { get; set; }
}
