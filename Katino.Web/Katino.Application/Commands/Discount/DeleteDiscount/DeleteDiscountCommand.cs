using MediatR;

namespace Katino.Application.Commands.DiscountN.DeleteDiscount;

public class DeleteDiscountCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
