using Katino.Store.Application.DTOs.Products;
using MediatR;

namespace Katino.Store.Application.Queries.Products.GetProductCard;

public class GetProductCardQuery : IRequest<ProductCardDto>
{
    public Guid Id { get; set; }
}
