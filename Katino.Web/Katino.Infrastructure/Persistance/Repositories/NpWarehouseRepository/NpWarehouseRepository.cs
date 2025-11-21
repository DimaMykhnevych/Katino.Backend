using Katino.Domain.Entities;
using Katino.Domain.Repositories.NpWarehouseRepository;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Katino.Infrastructure.Persistance.Repositories.NpWarehouseRepository;

public class NpWarehouseRepository : Repository<NpWarehouse>, INpWarehouseRepository
{
    public NpWarehouseRepository(KatinoDbContext context) : base(context)
    {
    }

    public async Task<int> GetInactiveWarehousesCountAsync()
    {
        return await context.NpWarehouses.Where(w => !w.IsActive).CountAsync();
    }

    public async Task<NpWarehouse> GetWarehouseByNumberAndCityRefAsync(string warehouseNumber, string cityRef)
    {
        return await context.NpWarehouses
            .FirstOrDefaultAsync(w => w.Number == warehouseNumber && w.CityRef == cityRef);
    }

    public async Task<NpWarehouse> GetWarehouseByRefAsync(string warehouseRef)
    {
        return await context.NpWarehouses.FirstOrDefaultAsync(w => w.Ref == warehouseRef);
    }

    public async Task<int> GetWarehousesCountAsync()
    {
        return await context.NpWarehouses.CountAsync();
    }

    public async Task SetWarehouseActiveAsync(bool isActive)
    {
        await context.NpWarehouses
            .ExecuteUpdateAsync(w => w.SetProperty(x => x.IsActive, isActive));
    }
}
