using AutoMapper;
using Katino.Application.DTOs.NovaPost;
using Katino.Domain.Services.NovaPost.City;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.NovaPost.GetNpCities;

public class GetNpCitiesQueryHandler : IRequestHandler<GetNpCitiesQuery, GetNpCitiesResponseDto>
{
    private const int CitiesLimit = 100;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly INpCityService _npCityService;

    public GetNpCitiesQueryHandler(
        INpCityService npCityService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _logger = loggerFactory?.CreateLogger(nameof(GetNpCitiesQueryHandler));
        _mapper = mapper;
        _npCityService = npCityService;
    }

    public async Task<GetNpCitiesResponseDto> Handle(GetNpCitiesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get NP cities query");
        ArgumentNullException.ThrowIfNull(request);

        var cities = await _npCityService.GetCities(request.CityName, CitiesLimit);
        return _mapper.Map<GetNpCitiesResponseDto>(cities);
    }
}
