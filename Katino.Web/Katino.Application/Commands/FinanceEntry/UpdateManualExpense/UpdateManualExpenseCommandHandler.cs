using AutoMapper;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceEntryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.FinanceEntryN.UpdateManualExpense;

public class UpdateManualExpenseCommandHandler : IRequestHandler<UpdateManualExpenseCommand, bool>
{
    private readonly IFinanceEntryRepository _repo;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateManualExpenseCommandHandler(
        IFinanceEntryRepository repo,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _repo = repo;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateManualExpenseCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateManualExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update manual expense request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.");
            }

            var entry = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (entry == null)
            {
                return false;
            }

            if (entry.IsLocked)
            {
                throw new InvalidOperationException("Entry is locked and cannot be modified.");
            }

            if (entry.SourceType != FinanceEntrySourceType.Manual || entry.Reason != FinanceEntryReason.Expense)
            {
                throw new InvalidOperationException("Only manual expense entries can be updated.");
            }

            entry.Amount = request.Amount;
            entry.Comment = request.Comment;
            entry.UpdatedAtUtc = DateTime.UtcNow;

            await _repo.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating manual expense");
            return false;
        }
    }
}
