namespace Katino.Domain.Entities;

public class DiscountBundleProduct
{
    public Guid DiscountId { get; set; }
    public Guid ProductId { get; set; }

    public Discount Discount { get; set; }
    public Product Product { get; set; }
}
