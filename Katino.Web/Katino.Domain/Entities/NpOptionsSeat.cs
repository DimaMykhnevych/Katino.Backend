using System.ComponentModel.DataAnnotations;

namespace Katino.Domain.Entities;

public class NpOptionsSeat
{
    public Guid Id { get; set; }

    [Required]
    public double VolumetricWidth { get; set; }

    [Required]
    public double VolumetricLength { get; set; }

    [Required]
    public double VolumetricHeight { get; set; }

    [Required]
    public double Weight { get; set; }

    public List<OrderNpOptionsSeat> OrderNpOptionsSeats { get; set; } = [];
}
