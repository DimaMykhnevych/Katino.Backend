namespace Katino.Domain.Entities;

public class ProductVariantSewer
{
    public Guid ProductVariantId { get; set; }
    public Guid SewerId { get; set; }

    public ProductVariant ProductVariant { get; set; }
    public AppUser Sewer { get; set; }
}
