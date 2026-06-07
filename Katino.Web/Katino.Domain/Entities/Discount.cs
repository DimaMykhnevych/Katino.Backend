using Katino.Domain.Enums;

namespace Katino.Domain.Entities;

public class Discount
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DiscountType Type { get; set; }
    public DiscountValueType ValueType { get; set; }
    public decimal Value { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<DiscountProduct> DiscountProducts { get; set; } = [];
    public List<DiscountCollection> DiscountCollections { get; set; } = [];
    public List<DiscountBundleProduct> BundleProducts { get; set; } = [];
}
