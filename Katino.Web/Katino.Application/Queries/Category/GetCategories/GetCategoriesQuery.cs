using Katino.Application.DTOs.Category;
using MediatR;

namespace Katino.Application.Queries.CategoryN.GetCategories;

public class GetCategoriesQuery : IRequest<GetCategoryDto>
{
    public string Name { get; set; }
}
