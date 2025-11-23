namespace Katino.Application.DTOs.NovaPost;

public class GetNpCitiesResponseDto
{
    public int TotalCount { get; set; }
    public IEnumerable<NpCityResponseDto> Addresses { get; set; }
}
