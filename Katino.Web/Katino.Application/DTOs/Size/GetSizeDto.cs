namespace Katino.Application.DTOs.Size;

public class GetSizeDto
{
    public IEnumerable<SizeDto> Sizes { get; set; }
    public int ResultsAmount { get; set; }
}
