using Katino.Application.DTOs.ProductVariant;
using MediatR;

namespace Katino.Application.Commands.ProductVariantN.UpdateProductVariant;

public class UpdateProductVariantCommand : IRequest<bool>
{
    public UpdateProductVariantDto ProductVariant { get; set; }
}
