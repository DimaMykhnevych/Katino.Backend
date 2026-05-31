using Katino.Store.Application.DTOs.Categories;
using MediatR;

namespace Katino.Store.Application.Queries.Categories.GetCategories;

public class GetCategoriesQuery : IRequest<IEnumerable<CategoryListItemDto>>
{
}
