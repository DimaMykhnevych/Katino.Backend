using AutoMapper;
using Katino.Application.DTOs.ProductVariantRedistributionHistory;
using Katino.Domain.Repositories.ProductVariantRedistributionHistoryRepository;
using MediatR;

namespace Katino.Application.Queries.ProductVariantRedistributionHistoryN.GetOrderRedistributionHistory;

public class GetOrderRedistributionHistoryQueryHandler : IRequestHandler<GetOrderRedistributionHistoryQuery, IEnumerable<ProductVariantRedistributionHistoryDto>>
{
    private readonly IProductVariantRedistributionHistoryRepository _historyRepository;
    private readonly IMapper _mapper;

    public GetOrderRedistributionHistoryQueryHandler(
        IProductVariantRedistributionHistoryRepository historyRepository,
        IMapper mapper)
    {
        _historyRepository = historyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductVariantRedistributionHistoryDto>> Handle(GetOrderRedistributionHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByOrderIdAsync(request.OrderId);
        return _mapper.Map<IEnumerable<ProductVariantRedistributionHistoryDto>>(history);
    }
}
