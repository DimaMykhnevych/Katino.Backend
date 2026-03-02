using Katino.Application.DTOs.FinanceCategory;
using MediatR;

namespace Katino.Application.Commands.FinanceCategoryN.AddFinanceCategory;

public class AddFinanceCategoryCommand : IRequest<FinanceCategoryDto>
{
    public FinanceCategoryTypeDto FinanceCategoryType { get; set; }
    public string Name { get; set; }
}
