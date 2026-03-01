using Katino.Application.DTOs.FinanceCategory;
using MediatR;

namespace Katino.Application.Queries.FinanceCategoryN.GetFinanceCategories;

public class GetFinanceCategoriesQuery : IRequest<IEnumerable<FinanceCategoryDto>>
{
    public FinanceCategoryTypeDto FinanceCategoryType { get; set; }
    public bool IncludeInactive { get; set; } = false;
}
