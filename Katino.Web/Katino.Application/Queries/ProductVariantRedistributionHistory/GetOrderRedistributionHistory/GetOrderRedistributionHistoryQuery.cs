using Katino.Application.DTOs.ProductVariantRedistributionHistory;
using MediatR;

namespace Katino.Application.Queries.ProductVariantRedistributionHistoryN.GetOrderRedistributionHistory;

public class GetOrderRedistributionHistoryQuery : IRequest<IEnumerable<ProductVariantRedistributionHistoryDto>>
{
    public Guid OrderId { get; set; }
}
