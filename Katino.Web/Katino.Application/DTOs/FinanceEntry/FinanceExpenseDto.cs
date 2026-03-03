namespace Katino.Application.DTOs.FinanceEntry;

public class FinanceExpenseDto
{
    public Guid Id { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Amount { get; set; }
    public string Comment { get; set; }

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;

    public bool IsLocked { get; set; }
}
