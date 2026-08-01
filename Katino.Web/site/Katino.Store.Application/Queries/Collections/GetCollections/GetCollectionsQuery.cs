using Katino.Store.Application.DTOs.Collections;
using MediatR;

namespace Katino.Store.Application.Queries.Collections.GetCollections;

public class GetCollectionsQuery : IRequest<IEnumerable<CollectionListItemDto>>
{
}
