using Katino.Application.DTOs.NpWarehouse;
using MediatR;

namespace Katino.Application.Queries.NpWarehouse.SearchNpWarehouses;

public class SearchNpWarehousesQuery : IRequest<IEnumerable<NpWarehouseDto>>
{
    public string CityRef { get; set; }
    public string SearchString { get; set; }
}
