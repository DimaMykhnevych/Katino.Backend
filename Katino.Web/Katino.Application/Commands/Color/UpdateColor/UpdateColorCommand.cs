using Katino.Application.DTOs.Color;
using MediatR;

namespace Katino.Application.Commands.ColorN.UpdateColor;

public class UpdateColorCommand : IRequest<ColorDto>
{
    public ColorDto Color { get; set; }
}
