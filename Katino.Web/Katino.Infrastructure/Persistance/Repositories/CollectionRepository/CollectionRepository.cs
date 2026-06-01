using Katino.Domain.Entities;
using Katino.Domain.Repositories.CollectionRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.CollectionRepository;

public class CollectionRepository : Repository<Collection>, ICollectionRepository
{
    public CollectionRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task DeleteProductsByCollectionId(Guid collectionId)
    {
        var productCollections = await context.ProductCollections
            .Where(pc => pc.CollectionId == collectionId)
            .ToListAsync();
        context.ProductCollections.RemoveRange(productCollections);
    }

    public async Task InsertProductCollections(IEnumerable<ProductCollection> productCollections)
    {
        await context.ProductCollections.AddRangeAsync(productCollections);
    }
}
