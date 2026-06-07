using Katino.Application.DTOs.Collection;
using Katino.Application.DTOs.Order;

namespace Katino.Application.DTOs.Discount;

public class DiscountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DiscountTypeDto Type { get; set; }
    public DiscountValueTypeDto ValueType { get; set; }
    public decimal Value { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductInCollectionDto> Products { get; set; } = [];
    public List<DiscountCollectionDto> Collections { get; set; } = [];
    public List<ProductInCollectionDto> BundleProducts { get; set; } = [];
}
