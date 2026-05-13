using AutoMapper;
using Katino.Application.DTOs.Statistics;
using Katino.Domain.Services.StatisticsN;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.Statistics.GetTopSellingProducts;

public class GetTopSellingProductsQueryHandler : IRequestHandler<GetTopSellingProductsQuery, GetTopSellingProductsDto>
{
    private readonly ITopSellingProductsService _service;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetTopSellingProductsQueryHandler(
        ITopSellingProductsService service,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _service = service;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(GetTopSellingProductsQueryHandler));
    }

    public async Task<GetTopSellingProductsDto> Handle(GetTopSellingProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get top selling products");
        ArgumentNullException.ThrowIfNull(request);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var result = await _service.GetAsync(page, pageSize, request.From, request.To, cancellationToken);
        return _mapper.Map<GetTopSellingProductsDto>(result);
    }
}
