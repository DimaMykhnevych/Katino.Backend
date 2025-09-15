using MediatR;

namespace Katino.Application.Commands.ProductN.UpdateProduct;

public class UpdateProductCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CategoryId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal DropPrice { get; set; }
    public decimal Price { get; set; }
}
