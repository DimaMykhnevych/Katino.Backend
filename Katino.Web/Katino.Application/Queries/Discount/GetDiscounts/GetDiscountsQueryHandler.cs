using AutoMapper;
using Katino.Application.DTOs.Discount;
using Katino.Domain.Repositories.DiscountRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.DiscountN.GetDiscounts;

public class GetDiscountsQueryHandler : IRequestHandler<GetDiscountsQuery, List<DiscountDto>>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetDiscountsQueryHandler(
        IDiscountRepository discountRepository,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _discountRepository = discountRepository;
        _logger = loggerFactory?.CreateLogger(nameof(GetDiscountsQueryHandler));
        _mapper = mapper;
    }

    public async Task<List<DiscountDto>> Handle(GetDiscountsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get discounts request");

        try
        {
            var discounts = await _discountRepository.GetAllWithDetailsAsync();
            return _mapper.Map<List<DiscountDto>>(discounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during getting discounts");
            return null;
        }
    }
}
