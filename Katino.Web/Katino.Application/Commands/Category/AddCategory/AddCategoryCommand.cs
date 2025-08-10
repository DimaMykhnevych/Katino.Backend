using MediatR;

namespace Katino.Application.Commands.CategoryN.AddCategory;

public class AddCategoryCommand : IRequest<bool>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
