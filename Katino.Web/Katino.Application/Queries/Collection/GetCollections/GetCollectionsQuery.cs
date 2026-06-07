using Katino.Application.DTOs.Collection;
using MediatR;

namespace Katino.Application.Queries.CollectionN.GetCollections;

public class GetCollectionsQuery : IRequest<List<CollectionDto>>
{
    public string Name { get; set; }
}
