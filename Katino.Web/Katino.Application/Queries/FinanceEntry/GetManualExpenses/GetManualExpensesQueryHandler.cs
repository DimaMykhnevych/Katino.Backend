using AutoMapper;
using Katino.Application.DTOs.FinanceEntry;
using Katino.Domain.Repositories.FinanceEntryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.FinanceEntryN.GetManualExpenses;

public class GetManualExpensesQueryHandler : IRequestHandler<GetManualExpensesQuery, List<FinanceExpenseDto>>
{
    private readonly IFinanceEntryRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetManualExpensesQueryHandler(
        IFinanceEntryRepository repo,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(GetManualExpensesQueryHandler));
    }

    public async Task<List<FinanceExpenseDto>> Handle(GetManualExpensesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get finance expenses");
        ArgumentNullException.ThrowIfNull(request);

        var year = request.Year ?? DateTime.UtcNow.Year;

        var entries = await _repo.GetManualExpensesByYearAsync(year, cancellationToken);
        return _mapper.Map<List<FinanceExpenseDto>>(entries);
    }
}
