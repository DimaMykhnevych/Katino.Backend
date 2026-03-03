using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Domain.Repositories.FinanceEntryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.FinanceEntryN.CreateManualExpense;

public class CreateManualExpenseCommandHandler : IRequestHandler<CreateManualExpenseCommand, bool>
{
    private readonly IFinanceEntryRepository _entryRepo;
    private readonly IFinanceCategoryRepository _catRepo;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public CreateManualExpenseCommandHandler(
        IFinanceEntryRepository entryRepo,
        IFinanceCategoryRepository catRepo,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _entryRepo = entryRepo;
        _catRepo = catRepo;
        _logger = loggerFactory?.CreateLogger(nameof(CreateManualExpenseCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(CreateManualExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling create manual expense request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0.");
            }

            var category = await _catRepo.Get(request.CategoryId);
            if (category == null)
            {
                throw new ArgumentException($"Category {request.CategoryId} not found.");
            }

            if (category.Type != FinanceCategoryType.Expense)
            {
                throw new ArgumentException("Category must be Expense type.");
            }

            var entry = new FinanceEntry
            {
                Id = Guid.NewGuid(),
                EntryDate = request.EntryDate.Date,
                Amount = request.Amount,
                Comment = request.Comment,

                SourceType = FinanceEntrySourceType.Manual,
                Reason = FinanceEntryReason.Expense,
                SaleType = null,
                IsLocked = false,

                CategoryId = request.CategoryId,

                CreatedBy = request.CreatedBy,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _entryRepo.Insert(entry);
            await _entryRepo.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during creating manual expense");
            return false;
        }
    }
}
