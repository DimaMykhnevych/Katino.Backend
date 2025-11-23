namespace Katino.Domain.Models.NovaPost;

public class GetCitiesResponse
{
    public int TotalCount { get; set; }
    public IEnumerable<CityResponse> Addresses { get; set; }
}
