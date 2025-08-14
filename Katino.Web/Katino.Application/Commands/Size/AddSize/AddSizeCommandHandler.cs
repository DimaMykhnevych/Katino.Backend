using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.SizeRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.SizeN.AddSize;

public class AddSizeCommandHandler : IRequestHandler<AddSizeCommand, bool>
{
    private readonly ISizeRepository _sizeRepository;
    private readonly ILogger _logger;

    public AddSizeCommandHandler(
        ISizeRepository sizeRepository,
        ILoggerFactory loggerFactory)
    {
        _sizeRepository = sizeRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddSizeCommandHandler));
    }

    public async Task<bool> Handle(AddSizeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add size request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var isNumeric = int.TryParse(request.Name, out _);
            Size size = new()
            {
                Name = request.Name,
                Type = isNumeric ? SizeType.Number : SizeType.Letter
            };

            await _sizeRepository.Insert(size);
            await _sizeRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product category");
            return false;
        }
    }
}
