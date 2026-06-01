using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.CollectionRepository;

public interface ICollectionRepository : IRepository<Collection>
{
    Task DeleteProductsByCollectionId(Guid collectionId);
    Task InsertProductCollections(IEnumerable<ProductCollection> productCollections);
}
