using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.ColorRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ColorN.UpdateColor;

public class UpdateColorCommandHandler : IRequestHandler<UpdateColorCommand, bool>
{
    private readonly IColorRepository _colorRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateColorCommandHandler(
        IColorRepository colorRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _colorRepository = colorRepository;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateColorCommandHandler));
    }

    public async Task<bool> Handle(UpdateColorCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update color request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var color = _mapper.Map<Color>(request.Color);

            await _colorRepository.Update(color);
            await _colorRepository.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating color");
            return false;
        }
    }
}
