using Katino.Store.Application.DTOs.Products;
using MediatR;

namespace Katino.Store.Application.Queries.Products.GetProducts;

public class GetProductsQuery : IRequest<GetProductsDto>
{
    public string Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
