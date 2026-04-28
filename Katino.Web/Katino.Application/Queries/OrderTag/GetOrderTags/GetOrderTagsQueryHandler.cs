using AutoMapper;
using Katino.Application.DTOs.OrderTag;
using Katino.Domain.Repositories.OrderTagRepository;
using MediatR;

namespace Katino.Application.Queries.OrderTag.GetOrderTags;

public class GetOrderTagsQueryHandler : IRequestHandler<GetOrderTagsQuery, IEnumerable<OrderTagDto>>
{
    private readonly IOrderTagRepository _orderTagRepository;
    private readonly IMapper _mapper;

    public GetOrderTagsQueryHandler(IOrderTagRepository orderTagRepository, IMapper mapper)
    {
        _orderTagRepository = orderTagRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderTagDto>> Handle(GetOrderTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _orderTagRepository.GetFilteredAsync(request.Search, request.CustomOnly);
        return _mapper.Map<IEnumerable<OrderTagDto>>(tags);
    }
}
