using Katino.Application.DTOs.Product;
using MediatR;

namespace Katino.Application.Commands.ProductN.AddProduct;

public class AddProductCommand : IRequest<ProductDto>
{
    public string Name { get; set; }
    public Guid CategoryId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal DropPrice { get; set; }
    public decimal Price { get; set; }
}
