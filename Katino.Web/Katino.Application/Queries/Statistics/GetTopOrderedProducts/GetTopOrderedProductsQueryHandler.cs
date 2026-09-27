using AutoMapper;
using Katino.Application.DTOs.Statistics;
using Katino.Domain.Constants;
using Katino.Domain.Enums;
using Katino.Domain.Models.Statistics;
using Katino.Domain.Services.StatisticsN;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.Statistics.GetTopOrderedProducts;

public class GetTopOrderedProductsQueryHandler : IRequestHandler<GetTopOrderedProductsQuery, GetTopSellingProductsDto>
{
    private readonly IProductSalesRankingService _service;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetTopOrderedProductsQueryHandler(
        IProductSalesRankingService service,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _service = service;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(GetTopOrderedProductsQueryHandler));
    }

    public async Task<GetTopSellingProductsDto> Handle(GetTopOrderedProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get top ordered products");
        ArgumentNullException.ThrowIfNull(request);

        var result = await _service.GetAsync(
            OrderStatusFilter.Except([OrderStatus.Exchange, OrderStatus.Refusal, .. InternetDocumentConstants.OrderRejectedStatuses]),
            request.Page,
            request.PageSize,
            request.From,
            request.To,
            cancellationToken);

        return _mapper.Map<GetTopSellingProductsDto>(result);
    }
}
