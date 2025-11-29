using Katino.Application.DTOs.NpCity;

namespace Katino.Application.DTOs.CrmUserSettings;

public class UpdateCrmUserSettingsDto
{
    public Guid Id { get; set; }
    public AddNpCityDto NpCity { get; set; }
    public Guid NpWarehouseId { get; set; }
}
