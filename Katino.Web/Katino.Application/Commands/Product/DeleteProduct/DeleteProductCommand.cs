using MediatR;

namespace Katino.Application.Commands.ProductN.DeleteProduct;

public class DeleteProductCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
