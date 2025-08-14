using MediatR;

namespace Katino.Application.Commands.CategoryN.DeleteCategory;

public class DeleteCategoryCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
