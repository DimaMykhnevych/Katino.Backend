using MediatR;

namespace Katino.Application.Commands.ProductN.AddProduct;

public class AddProductCommand : IRequest<bool>
{
    public string Name { get; set; }
    public string Article { get; set; }
    public Guid CategoryId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal DropPrice { get; set; }
    public decimal Price { get; set; }
}
