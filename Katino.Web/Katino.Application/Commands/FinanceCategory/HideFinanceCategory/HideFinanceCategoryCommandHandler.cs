using Katino.Domain.Repositories.FinanceCategoryRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.FinanceCategoryN.HideFinanceCategory;

public class HideFinanceCategoryCommandHandler : IRequestHandler<HideFinanceCategoryCommand, bool>
{
    private readonly IFinanceCategoryRepository _repo;
    private readonly ILogger _logger;

    public HideFinanceCategoryCommandHandler(
        IFinanceCategoryRepository repo,
        ILoggerFactory loggerFactory)
    {
        _repo = repo;
        _logger = loggerFactory?.CreateLogger(nameof(HideFinanceCategoryCommandHandler));
    }

    public async Task<bool> Handle(HideFinanceCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling hide finance category request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var entity = await _repo.Get(request.Id);
            if (entity == null)
            {
                return false;
            }

            if (!entity.IsActive)
            {
                return true;
            }

            entity.IsActive = false;

            await _repo.Update(entity);
            await _repo.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during hiding finance category");
            return false;
        }
    }
}
