namespace Katino.Application.DTOs.OrderNpOptionsSeat;

public class OrderNpOptionsSeatDto
{
    public Guid Id { get; set; }
    public Guid NpOptionsSeatId { get; set; }

    public double VolumetricWidth { get; set; }
    public double VolumetricLength { get; set; }
    public double VolumetricHeight { get; set; }
    public double Weight { get; set; }
}
