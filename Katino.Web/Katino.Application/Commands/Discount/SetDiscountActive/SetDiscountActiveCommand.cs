using MediatR;

namespace Katino.Application.Commands.DiscountN.SetDiscountActive;

public class SetDiscountActiveCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
