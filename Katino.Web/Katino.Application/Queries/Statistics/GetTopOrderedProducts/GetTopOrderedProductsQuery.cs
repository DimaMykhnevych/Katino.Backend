using Katino.Application.DTOs.Statistics;
using MediatR;

namespace Katino.Application.Queries.Statistics.GetTopOrderedProducts;

public class GetTopOrderedProductsQuery : IRequest<GetTopSellingProductsDto>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
}
