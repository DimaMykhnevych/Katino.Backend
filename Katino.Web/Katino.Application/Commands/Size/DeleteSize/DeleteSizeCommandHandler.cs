using Katino.Domain.Entities;
using Katino.Domain.Repositories.SizeRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.SizeN.DeleteSize;

public class DeleteSizeCommandHandler : IRequestHandler<DeleteSizeCommand, bool>
{
    private readonly ISizeRepository _sizeRepository;
    private readonly ILogger _logger;

    public DeleteSizeCommandHandler(
        ISizeRepository sizeRepository,
        ILoggerFactory loggerFactory)
    {
        _sizeRepository = sizeRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteSizeCommandHandler));
    }

    public async Task<bool> Handle(DeleteSizeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete size request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Size sizeFromDb = await _sizeRepository.Get(request.Id);
            if (sizeFromDb == null)
            {
                throw new ArgumentException($"Size with id {request.Id} doesn't exist");
            }

            _sizeRepository.Delete(sizeFromDb);
            await _sizeRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting size");
            return false;
        }
    }
}
