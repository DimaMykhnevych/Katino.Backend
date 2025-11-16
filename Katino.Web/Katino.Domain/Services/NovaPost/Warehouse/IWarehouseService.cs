using Katino.Domain.Models.NovaPost;

namespace Katino.Domain.Services.NovaPost.Warehouse;

public interface IWarehouseService
{
    Task<WarehousesResponse> SearchWarehousesAsync(string cityRef, string warehouseId);
    Task<IEnumerable<WarehousesResponse>> GetWarehousesWithPaginationAsync(string page, string limit);
}
