using Katino.Store.Application.DTOs.Products;
using MediatR;

namespace Katino.Store.Application.Queries.Products.GetRecentProducts;

public class GetRecentProductsQuery : IRequest<IEnumerable<ProductListItemDto>>
{
}
