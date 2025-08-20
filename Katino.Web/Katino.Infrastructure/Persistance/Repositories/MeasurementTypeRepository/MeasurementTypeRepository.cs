using Katino.Domain.Entities;
using Katino.Domain.Repositories.MeasurementTypeRepository;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Infrastructure.Persistance.Repositories.MeasurementTypeRepository;

public class MeasurementTypeRepository : Repository<MeasurementType>, IMeasurementTypeRepository
{
    public MeasurementTypeRepository(KatinoDbContext context) : base(context)
    {
    }
}
