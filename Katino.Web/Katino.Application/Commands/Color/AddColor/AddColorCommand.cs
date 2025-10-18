using Katino.Application.DTOs.Color;
using MediatR;

namespace Katino.Application.Commands.ColorN.AddColor;

public class AddColorCommand : IRequest<ColorDto>
{
    public string Name { get; set; }
    public string HexCode { get; set; }
}
