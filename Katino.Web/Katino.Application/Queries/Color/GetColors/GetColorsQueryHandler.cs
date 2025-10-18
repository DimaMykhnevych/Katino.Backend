using AutoMapper;
using Katino.Application.DTOs.Color;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.ColorN.GetColors;

public class GetColorsQueryHandler : IRequestHandler<GetColorsQuery, GetColorDto>
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetColorsQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetColorsQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetColorDto> Handle(GetColorsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get colors");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<Color> colors = _katinoDbContext.Colors.AsNoTracking();
        if (!string.IsNullOrEmpty(request.Name))
        {
            colors = colors.Where(p => p.Name.Contains(request.Name));
        }

        var resultColors = await colors.OrderBy(c => c.Name).ToListAsync(cancellationToken);
        List<ColorDto> colorDtos =
            _mapper.Map<IEnumerable<ColorDto>>(resultColors)
                .ToList();

        return new GetColorDto
        {
            Colors = colorDtos,
            ResultsAmount = colorDtos.Count
        };
    }
}
