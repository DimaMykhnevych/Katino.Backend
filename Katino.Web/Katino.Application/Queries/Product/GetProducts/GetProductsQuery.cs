using Katino.Application.DTOs.Product;
using MediatR;

namespace Katino.Application.Queries.ProductN.GetProducts;

public class GetProductsQuery : IRequest<GetProductDto>
{
    public string Name { get; set; }
}
