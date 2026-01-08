namespace Katino.Domain.Models;

public class SewedReport
{
    public Guid ProductVariantId { get; init; }
    public int ActualSewedQuantity { get; init; }
    public Guid? OrderItemId { get; init; }
}
