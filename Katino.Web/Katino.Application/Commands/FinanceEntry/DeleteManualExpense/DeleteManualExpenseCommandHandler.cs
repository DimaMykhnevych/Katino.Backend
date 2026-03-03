using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceEntryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.FinanceEntryN.DeleteManualExpense;

public class DeleteManualExpenseCommandHandler : IRequestHandler<DeleteManualExpenseCommand, bool>
{
    private readonly IFinanceEntryRepository _repo;
    private readonly ILogger _logger;

    public DeleteManualExpenseCommandHandler(
        IFinanceEntryRepository repo,
        ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteManualExpenseCommandHandler));
    }

    public async Task<bool> Handle(DeleteManualExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete manual expense request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var entry = await _repo.GetByIdAsync(request.Id, cancellationToken);
            if (entry == null)
            {
                return false;
            }

            if (entry.IsLocked)
            {
                throw new InvalidOperationException("Entry is locked and cannot be deleted.");
            }

            if (entry.SourceType != FinanceEntrySourceType.Manual || entry.Reason != FinanceEntryReason.Expense)
            {
                throw new InvalidOperationException("Only manual expense entries can be deleted.");
            }

            _repo.Delete(entry);
            await _repo.Save();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting manual expense");
            return false;
        }
    }
}
