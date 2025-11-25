namespace Katino.Domain.Entities;

public class CrmUserSettings
{
    public Guid Id { get; set; }
    public Guid AppUserId { get; set; }
    public Guid NpCityId { get; set; }
    public Guid NpWarehouseId { get; set; }

    // Navigation properties
    public AppUser AppUser { get; set; }
    public virtual NpCity NpCity { get; set; }
    public virtual NpWarehouse NpWarehouse { get; set; }
}
