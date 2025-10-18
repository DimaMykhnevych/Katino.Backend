using AutoMapper;
using Katino.Application.DTOs.Color;
using Katino.Domain.Entities;
using Katino.Domain.Repositories.ColorRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.ColorN.AddColor;

public class AddColorCommandHandler : IRequestHandler<AddColorCommand, ColorDto>
{
    private readonly IColorRepository _colorRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddColorCommandHandler(
        IColorRepository colorRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _colorRepository = colorRepository;
        _logger = loggerFactory?.CreateLogger(nameof(AddColorCommandHandler));
        _mapper = mapper;
    }

    public async Task<ColorDto> Handle(AddColorCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add product color request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            Color color = _mapper.Map<Color>(request);
            var addedColor = await _colorRepository.Insert(color);
            await _colorRepository.Save();
            return _mapper.Map<ColorDto>(addedColor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding product color");
            return null;
        }
    }
}
