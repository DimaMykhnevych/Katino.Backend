using AutoMapper;
using Katino.Application.DTOs.Order;
using Katino.Domain.Enums;
using Katino.Domain.Models.Pricing;
using Katino.Domain.Services.OrderN.OrderPricingService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.OrderN.PreviewOrderCost;

public class PreviewOrderCostQueryHandler : IRequestHandler<PreviewOrderCostQuery, OrderPricingResultDto>
{
    private readonly IOrderPricingService _orderPricingService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public PreviewOrderCostQueryHandler(
        IOrderPricingService orderPricingService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _orderPricingService = orderPricingService;
        _logger = loggerFactory?.CreateLogger(nameof(PreviewOrderCostQueryHandler));
        _mapper = mapper;
    }

    public async Task<OrderPricingResultDto> Handle(PreviewOrderCostQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling preview order cost request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            SaleType saleType = _mapper.Map<SaleType>(request.SaleType);
            IReadOnlyList<OrderPricingRequest> pricingRequests = _mapper.Map<List<OrderPricingRequest>>(request.Items);

            OrderPricingResult result = await _orderPricingService.CalculateByVariantIdsAsync(pricingRequests, saleType);
            return _mapper.Map<OrderPricingResultDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during order cost preview");
            return new();
        }
    }
}
