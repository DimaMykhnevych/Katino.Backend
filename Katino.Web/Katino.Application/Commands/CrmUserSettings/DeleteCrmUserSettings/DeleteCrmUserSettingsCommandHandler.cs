using Katino.Domain.Repositories.CrmUserSettingsRepository;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CrmUserSettingsN.DeleteCrmUserSettings;

public class DeleteCrmUserSettingsCommandHandler : IRequestHandler<DeleteCrmUserSettingsCommand, bool>
{
    private readonly ICrmUserSettingsRepository _crmUserSettingsRepository;
    private readonly ILogger _logger;

    public DeleteCrmUserSettingsCommandHandler(
        ICrmUserSettingsRepository crmUserSettingsRepository,
        ILoggerFactory loggerFactory)
    {
        _crmUserSettingsRepository = crmUserSettingsRepository;
        _logger = loggerFactory?.CreateLogger(nameof(DeleteCrmUserSettingsCommandHandler));
    }

    public async Task<bool> Handle(DeleteCrmUserSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling delete CRM user settings request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var userSettingsFromDb = await _crmUserSettingsRepository.Get(request.Id);
            if (userSettingsFromDb == null)
            {
                throw new ArgumentException($"CRM user settings with id {request.Id} doesn't exist");
            }

            if (userSettingsFromDb.AppUserId != request.AppUserId)
            {
                throw new InvalidOperationException($"User {request.AppUserId} tried to delete settings for user {userSettingsFromDb.AppUserId}");
            }

            _crmUserSettingsRepository.Delete(userSettingsFromDb);
            await _crmUserSettingsRepository.Save();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during deleting CRM user settings");
            return false;
        }
    }
}
