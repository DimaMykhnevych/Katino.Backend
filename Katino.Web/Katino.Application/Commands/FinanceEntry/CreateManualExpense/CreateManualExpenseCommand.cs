using MediatR;

namespace Katino.Application.Commands.FinanceEntryN.CreateManualExpense;

public class CreateManualExpenseCommand : IRequest<bool>
{
    public Guid CreatedBy { get; set; }
    public DateTime EntryDate { get; set; }
    public decimal Amount { get; set; }
    public string Comment { get; set; }
    public Guid CategoryId { get; set; }
}
