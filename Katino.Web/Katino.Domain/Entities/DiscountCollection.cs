namespace Katino.Domain.Entities;

public class DiscountCollection
{
    public Guid DiscountId { get; set; }
    public Guid CollectionId { get; set; }

    public Discount Discount { get; set; }
    public Collection Collection { get; set; }
}
