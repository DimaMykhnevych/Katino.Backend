using MediatR;

namespace Katino.Application.Commands.FinanceCategoryN.HideFinanceCategory;

public class HideFinanceCategoryCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
