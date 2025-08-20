using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.MeasurementTypeRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.MeasurementTypeN.UpdateMeasurementType;

public class UpdateMeasurementTypeCommandHandler : IRequestHandler<UpdateMeasurementTypeCommand, bool>
{
    private readonly IMeasurementTypeRepository _measurementTypeRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateMeasurementTypeCommandHandler(
        IMeasurementTypeRepository measurementTypeRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _measurementTypeRepository = measurementTypeRepository;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateMeasurementTypeCommandHandler));
    }

    public async Task<bool> Handle(UpdateMeasurementTypeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update measurement type request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var measurementType = _mapper.Map<MeasurementType>(request.MeasurementType);

            await _measurementTypeRepository.Update(measurementType);
            await _measurementTypeRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating measurement type");
            return false;
        }
    }
}
