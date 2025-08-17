using Katino.Application.DTOs.Color;
using MediatR;

namespace Katino.Application.Commands.ColorN.UpdateColor;

public class UpdateColorCommand : IRequest<bool>
{
    public ColorDto Color { get; set; }
}
