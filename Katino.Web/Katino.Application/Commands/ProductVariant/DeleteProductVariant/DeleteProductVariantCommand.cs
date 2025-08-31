using MediatR;

namespace Katino.Application.Commands.ProductVariantN.DeleteProductVariant;

public class DeleteProductVariantCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
