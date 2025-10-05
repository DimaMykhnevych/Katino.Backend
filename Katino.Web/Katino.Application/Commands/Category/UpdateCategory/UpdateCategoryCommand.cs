using Katino.Application.DTOs.Category;
using MediatR;

namespace Katino.Application.Commands.CategoryN.UpdateCategory;

public class UpdateCategoryCommand : IRequest<CategoryDto>
{
    public CategoryDto Category { get; set; }
}
