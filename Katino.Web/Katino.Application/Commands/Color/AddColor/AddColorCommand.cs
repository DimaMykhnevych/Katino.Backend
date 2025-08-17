using MediatR;

namespace Katino.Application.Commands.ColorN.AddColor;

public class AddColorCommand : IRequest<bool>
{
    public string Name { get; set; }
    public string HexCode { get; set; }
}
