using Katino.Application.DTOs.Size;
using MediatR;

namespace Katino.Application.Commands.SizeN.AddSize;

public class AddSizeCommand : IRequest<SizeDto>
{
    public string Name { get; set; }
}
