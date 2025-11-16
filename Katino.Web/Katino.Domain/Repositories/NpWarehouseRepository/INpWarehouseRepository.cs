using Katino.Domain.Entities;

namespace Katino.Domain.Repositories.NpWarehouseRepository;

public interface INpWarehouseRepository : IRepository<NpWarehouse>
{
    Task<NpWarehouse> GetWarehouseByRefAsync(string warehouseRef);

    Task SetWarehouseActiveAsync(bool isActive);

    Task<int> GetWarehousesCountAsync();

    Task<int> GetInactiveWarehousesCountAsync();
}
