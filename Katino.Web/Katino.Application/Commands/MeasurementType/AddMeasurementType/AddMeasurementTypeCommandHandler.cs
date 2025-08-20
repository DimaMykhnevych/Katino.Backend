using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.MeasurementTypeRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.MeasurementTypeN.AddMeasurementType;

public class AddMeasurementTypeCommandHandler : IRequestHandler<AddMeasurementTypeCommand, bool>
{
    private readonly IMeasurementTypeRepository _measurementTypeRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddMeasurementTypeCommandHandler(
        IMeasurementTypeRepository measurementTypeRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _measurementTypeRepository = measurementTypeRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddMeasurementTypeCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(AddMeasurementTypeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add product measurement type request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            MeasurementType measurementType = _mapper.Map<MeasurementType>(request);
            await _measurementTypeRepository.Insert(measurementType);
            await _measurementTypeRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product measurement type");
            return false;
        }
    }
}
