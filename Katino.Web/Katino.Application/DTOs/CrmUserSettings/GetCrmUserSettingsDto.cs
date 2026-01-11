using Katino.Application.DTOs.NpCity;
using Katino.Application.DTOs.NpWarehouse;

namespace Katino.Application.DTOs.CrmUserSettings;

public class GetCrmUserSettingsDto
{
    public Guid? Id { get; set; }
    public GetNpCityDto NpCity { get; set; }
    public NpWarehouseDto NpWarehouse { get; set; }
}
