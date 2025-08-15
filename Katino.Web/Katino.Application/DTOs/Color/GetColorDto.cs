namespace Katino.Application.DTOs.Color;

public class GetColorDto
{
    public IEnumerable<ColorDto> Colors { get; set; }
    public int ResultsAmount { get; set; }
}
