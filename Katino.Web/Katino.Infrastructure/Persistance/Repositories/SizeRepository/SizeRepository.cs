using Katino.Domain.Entities;
using Katino.Domain.Repositories.SizeRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.SizeRepository;

public class SizeRepository : Repository<Size>, ISizeRepository
{
    public SizeRepository(KatinoDbContext context) : base(context)
    {
    }
}
