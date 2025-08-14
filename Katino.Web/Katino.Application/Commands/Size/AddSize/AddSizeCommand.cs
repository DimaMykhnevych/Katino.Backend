using MediatR;

namespace Katino.Application.Commands.SizeN.AddSize;

public class AddSizeCommand : IRequest<bool>
{
    public string Name { get; set; }
}
