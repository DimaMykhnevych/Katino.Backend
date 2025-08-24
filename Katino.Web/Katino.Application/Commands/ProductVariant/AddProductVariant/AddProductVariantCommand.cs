using Katino.Application.DTOs.ProductVariant;
using MediatR;

namespace Katino.Application.Commands.ProductVariantN.AddProductVariant;

public class AddProductVariantCommand : IRequest<bool>
{
    public AddProductVariantDto ProductVariant { get; set; }
}
