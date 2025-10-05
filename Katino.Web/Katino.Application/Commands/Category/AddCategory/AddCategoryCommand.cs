using Katino.Application.DTOs.Category;
using MediatR;

namespace Katino.Application.Commands.CategoryN.AddCategory;

public class AddCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
