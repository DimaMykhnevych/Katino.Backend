using MediatR;

namespace Katino.Application.Commands.SizeN.DeleteSize;

public class DeleteSizeCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
