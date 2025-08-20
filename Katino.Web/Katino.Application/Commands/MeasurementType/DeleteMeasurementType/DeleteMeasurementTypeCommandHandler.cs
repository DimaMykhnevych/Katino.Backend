using Katino.Application.Commands.ColorN.DeleteColor;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.MeasurementTypeRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.MeasurementTypeN.DeleteMeasurementType;

public class DeleteMeasurementTypeCommandHandler : IRequestHandler<DeleteMeasurementTypeCommand, bool>
{
    private readonly IMeasurementTypeRepository _measurementTypeRepository;
    private readonly ILogger _logger;

    public DeleteMeasurementTypeCommandHandler(
        IMeasurementTypeRepository measurementTypeRepository,
        ILoggerFactory loggerFactory)
    {
        _measurementTypeRepository = measurementTypeRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteColorCommandHandler));
    }

    public async Task<bool> Handle(DeleteMeasurementTypeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete measurement type request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            MeasurementType measurementTypeFromDb = await _measurementTypeRepository.Get(request.Id);
            if (measurementTypeFromDb == null)
            {
                throw new ArgumentException($"Measurement type with id {request.Id} doesn't exist");
            }

            _measurementTypeRepository.Delete(measurementTypeFromDb);
            await _measurementTypeRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting measurement type");
            return false;
        }
    }
}
