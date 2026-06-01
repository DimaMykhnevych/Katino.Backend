using Katino.Application.DTOs.Collection;
using MediatR;

namespace Katino.Application.Commands.CollectionN.UpdateCollectionProducts;

public class UpdateCollectionProductsCommand : IRequest<CollectionDto>
{
    public Guid CollectionId { get; set; }
    public List<Guid> ProductIds { get; set; }
}
