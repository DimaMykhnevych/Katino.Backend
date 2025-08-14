using Katino.Domain.Enums;
using Katino.Domain.Repositories.SizeRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.SizeN.UpdateSize;

public class UpdateSizeCommandHandler : IRequestHandler<UpdateSizeCommand, bool>
{
    private readonly ISizeRepository _sizeRepository;
    private readonly ILogger _logger;

    public UpdateSizeCommandHandler(
        ISizeRepository sizeRepository,
        ILoggerFactory loggerFactory)
    {
        _sizeRepository = sizeRepository;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateSizeCommandHandler));
    }

    public async Task<bool> Handle(UpdateSizeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update size request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var sizeFromDb = await _sizeRepository.Get(request.Id);
            sizeFromDb.Name = request.Name;

            var isNumeric = int.TryParse(request.Name, out _);
            sizeFromDb.Type = isNumeric ? SizeType.Number : SizeType.Letter;

            await _sizeRepository.Update(sizeFromDb);
            await _sizeRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating size");
            return false;
        }
    }
}
