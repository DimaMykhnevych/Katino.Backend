using AutoMapper;
using Katino.Application.DTOs.FinanceCategory;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.FinanceCategoryN.GetFinanceCategories;

public class GetFinanceCategoriesQueryHandler : IRequestHandler<GetFinanceCategoriesQuery, IEnumerable<FinanceCategoryDto>>
{
    private readonly IFinanceCategoryRepository _repo;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public GetFinanceCategoriesQueryHandler(
        IFinanceCategoryRepository repo,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _repo = repo;
        _logger = loggerFactory?.CreateLogger(nameof(GetFinanceCategoriesQueryHandler));
        _mapper = mapper;
    }

    public async Task<IEnumerable<FinanceCategoryDto>> Handle(GetFinanceCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get finance categories");
        ArgumentNullException.ThrowIfNull(request);

        var categoryType = _mapper.Map<FinanceCategoryType>(request.FinanceCategoryType);
        if (!request.IncludeInactive)
        {
            var list = await _repo.GetActiveByTypeAsync(categoryType);
            return _mapper.Map<IEnumerable<FinanceCategoryDto>>(list);
        }

        var all = await _repo.GetAll();
        var allFiltered = all
            .Where(x => x.Type == categoryType)
            .OrderBy(x => x.Name)
            .ToList();

        return _mapper.Map<IEnumerable<FinanceCategoryDto>>(allFiltered);
    }
}

