using Katino.Domain.Entities;
using Katino.Domain.Repositories.SewingHistoryRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.SewingHistoryRepository;

public class SewingHistoryRepository : Repository<SewingHistory>, ISewingHistoryRepository
{
    public SewingHistoryRepository(KatinoDbContext context) : base(context)
    {
    }
}
