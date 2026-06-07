using Katino.Application.DTOs.Discount;
using MediatR;

namespace Katino.Application.Queries.DiscountN.GetDiscounts;

public class GetDiscountsQuery : IRequest<List<DiscountDto>>
{
}
