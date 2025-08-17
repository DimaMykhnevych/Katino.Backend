using Katino.Domain.Entities;
using Katino.Domain.Repositories.ColorRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ColorN.DeleteColor;

public class DeleteColorCommandHandler : IRequestHandler<DeleteColorCommand, bool>
{
    private readonly IColorRepository _colorRepository;
    private readonly ILogger _logger;

    public DeleteColorCommandHandler(
        IColorRepository colorRepository,
        ILoggerFactory loggerFactory)
    {
        _colorRepository = colorRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteColorCommandHandler));
    }

    public async Task<bool> Handle(DeleteColorCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete color request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Color colorFromDb = await _colorRepository.Get(request.Id);
            if (colorFromDb == null)
            {
                throw new ArgumentException($"Color with id {request.Id} doesn't exist");
            }

            _colorRepository.Delete(colorFromDb);
            await _colorRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting color");
            return false;
        }
    }
}
