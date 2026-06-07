using Katino.Application.DTOs.Discount;
using MediatR;

namespace Katino.Application.Commands.DiscountN.UpdateDiscount;

public class UpdateDiscountCommand : IRequest<DiscountDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DiscountValueTypeDto ValueType { get; set; }
    public decimal Value { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Guid> ProductIds { get; set; } = [];
    public List<Guid> CollectionIds { get; set; } = [];
    public List<Guid> BundleProductIds { get; set; } = [];
}
