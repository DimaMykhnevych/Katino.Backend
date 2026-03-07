namespace Katino.Application.DTOs.OrderItem;

public class SubmitSewedReportItemDto
{
    public Guid ProductVariantId { get; init; }
    public int ActualSewedQuantity { get; init; }
    public Guid? OrderItemId { get; init; }
    public Guid SubmittedBy { get; set; }
}
