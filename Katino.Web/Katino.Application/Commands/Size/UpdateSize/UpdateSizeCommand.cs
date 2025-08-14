using MediatR;

namespace Katino.Application.Commands.SizeN.UpdateSize;

public class UpdateSizeCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public string Name { get; set; }
}
