using AutoMapper;
using Katino.Application.DTOs.NpWarehouse;
using Katino.Domain.Repositories.NpWarehouseRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.NpWarehouse.SearchNpWarehouses;

public class SearchNpWarehousesQueryHandler : IRequestHandler<SearchNpWarehousesQuery, IEnumerable<NpWarehouseDto>>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly INpWarehouseRepository _npWarehouseRepository;

    public SearchNpWarehousesQueryHandler(
        INpWarehouseRepository npWarehouseRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _logger = loggerFactory?.CreateLogger(nameof(SearchNpWarehousesQueryHandler));
        _mapper = mapper;
        _npWarehouseRepository = npWarehouseRepository;
    }

    public async Task<IEnumerable<NpWarehouseDto>> Handle(SearchNpWarehousesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling search NP warehouse query");
        ArgumentNullException.ThrowIfNull(request);

        var warehouses = await _npWarehouseRepository.SearchWarehouseBySearchStringAsync(request.CityRef, request.SearchString);
        return _mapper.Map<IEnumerable<NpWarehouseDto>>(warehouses);
    }
}
