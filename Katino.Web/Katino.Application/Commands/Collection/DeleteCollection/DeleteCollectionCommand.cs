using MediatR;

namespace Katino.Application.Commands.CollectionN.DeleteCollection;

public class DeleteCollectionCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
