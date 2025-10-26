using Katino.Domain.Entities;
using Katino.Domain.Repositories.ProductPhotoRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.ProductPhotoRepository;

public class ProductPhotoRepository : Repository<ProductPhoto>, IProductPhotoRepository
{
    public ProductPhotoRepository(KatinoDbContext context) : base(context)
    {
    }
}
