using MediatR;

namespace Katino.Application.Commands.FinanceEntryN.DeleteManualExpense;

public class DeleteManualExpenseCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
