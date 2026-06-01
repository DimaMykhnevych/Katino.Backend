using Katino.Application.DTOs.Collection;
using MediatR;

namespace Katino.Application.Commands.CollectionN.AddCollection;

public class AddCollectionCommand : IRequest<CollectionDto>
{
    public string Name { get; set; }
    public string Description { get; set; }
}
