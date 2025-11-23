using Katino.Application.DTOs.NovaPost;
using MediatR;

namespace Katino.Application.Queries.NovaPost.GetNpCities;

public class GetNpCitiesQuery : IRequest<GetNpCitiesResponseDto>
{
    public string CityName { get; set; }
}
