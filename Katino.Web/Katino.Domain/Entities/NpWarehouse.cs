using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

public class NpWarehouse
{
    public Guid Id { get; set; }

    [Required]
    public string Ref { get; set; }

    [Required]
    public string CityRef { get; set; }

    [Required]
    public string WarehouseIndex { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Number { get; set; }

    [Required]
    public string ShortAddress { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}
