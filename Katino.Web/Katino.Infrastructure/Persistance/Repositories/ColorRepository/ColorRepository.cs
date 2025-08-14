using Katino.Domain.Entities;
using Katino.Domain.Repositories.ColorRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.ColorRepository;

public class ColorRepository : Repository<Color>, IColorRepository
{
    public ColorRepository(KatinoDbContext context) : base(context)
    {
    }
}
