using Katino.Application.DTOs.NpCity;

namespace Katino.Application.DTOs.CrmUserSettings;

public class AddCrmUserSettingsDto
{
    public AddNpCityDto NpCity { get; set; }
    public Guid NpWarehouseId { get; set; }
}
