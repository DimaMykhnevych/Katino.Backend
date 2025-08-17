using MediatR;

namespace Katino.Application.Commands.ColorN.DeleteColor;

public class DeleteColorCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
