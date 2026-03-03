using Katino.Application.DTOs.FinanceEntry;
using MediatR;

namespace Katino.Application.Queries.FinanceEntryN.GetManualExpenses;

public class GetManualExpensesQuery : IRequest<List<FinanceExpenseDto>>
{
    public int? Year { get; set; }
}
