using Katino.Application.DTOs.Order;
using MediatR;

namespace Katino.Application.Queries.OrderN.PreviewOrderCost;

public class PreviewOrderCostQuery : IRequest<OrderPricingResultDto>
{
    public List<OrderPricingRequestDto> Items { get; set; } = [];
    public SaleTypeDto SaleType { get; set; }
}
