using AutoMapper;
using Katino.Application.DTOs.MeasurementType;
using Katino.Domain.Context;
using Katino.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.MeasurementTypeN.GetMeasurementTypes;

public class GetMeasurementTypesQueryHandler : IRequestHandler<GetMeasurementTypesQuery, GetMeasurementTypeDto>
{
    private readonly IKatinoDbContext _katinoDbContext;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetMeasurementTypesQueryHandler(
        IKatinoDbContext katinoDbContext,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _katinoDbContext = katinoDbContext;
        _logger = loggerFactory?.CreateLogger(nameof(GetMeasurementTypesQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetMeasurementTypeDto> Handle(GetMeasurementTypesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get measurement types");
        ArgumentNullException.ThrowIfNull(request);

        IQueryable<MeasurementType> measurementTypes = _katinoDbContext.MeasurementTypes.AsNoTracking();
        if (!string.IsNullOrEmpty(request.Name))
        {
            measurementTypes = measurementTypes.Where(p => p.Name.Contains(request.Name));
        }

        var resultMeasurementTypes = await measurementTypes.ToListAsync(cancellationToken);
        List<MeasurementTypeDto> mewMeasurementTypeDtos =
            _mapper.Map<IEnumerable<MeasurementTypeDto>>(resultMeasurementTypes)
                .ToList();

        return new GetMeasurementTypeDto
        {
            MeasurementTypes = mewMeasurementTypeDtos,
            ResultsAmount = mewMeasurementTypeDtos.Count
        };
    }
}
