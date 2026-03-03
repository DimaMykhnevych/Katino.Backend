using MediatR;

namespace Katino.Application.Commands.FinanceEntryN.UpdateManualExpense;

public class UpdateManualExpenseCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Comment { get; set; }
}
