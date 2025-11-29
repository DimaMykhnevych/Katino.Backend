using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Services.CrmUserSettingsN.UpdateCrmUserSettingsService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CrmUserSettingsN.UpdateCrmUserSettings;

public class UpdateCrmUserSettingsCommandHandler : IRequestHandler<UpdateCrmUserSettingsCommand, bool>
{
    private readonly IUpdateCrmUserSettingsService _updateCrmUserSettingsService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UpdateCrmUserSettingsCommandHandler(
        IUpdateCrmUserSettingsService updateCrmUserSettingsService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _updateCrmUserSettingsService = updateCrmUserSettingsService;
        _logger = loggerFactory?.CreateLogger(nameof(UpdateCrmUserSettingsCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateCrmUserSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling update CRM user settings request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            CrmUserSettings crmUserSettings = _mapper.Map<CrmUserSettings>(request.UserSettings);
            crmUserSettings.AppUserId = request.AppUserId;
            return await _updateCrmUserSettingsService.UpdateCrmUserSettingsAsync(crmUserSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during updating CRM user settings");
            return false;
        }
    }
}
