using AutoMapper;
using Katino.Domain.Entities;
using Katino.Domain.Services.CrmUserSettingsN.AddCrmUserSettingsService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Commands.CrmUserSettingsN.AddCrmUserSettings;

public class AddCrmUserSettingsCommandHandler : IRequestHandler<AddCrmUserSettingsCommand, bool>
{
    private readonly IAddCrmUserSettingsService _addCrmUserSettingsService;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public AddCrmUserSettingsCommandHandler(
        IAddCrmUserSettingsService addCrmUserSettingsService,
        ILoggerFactory loggerFactory,
        IMapper mapper)
    {
        _addCrmUserSettingsService = addCrmUserSettingsService;
        _logger = loggerFactory?.CreateLogger(nameof(AddCrmUserSettingsCommandHandler));
        _mapper = mapper;
    }

    public async Task<bool> Handle(AddCrmUserSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling add CRM user settings request");
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            CrmUserSettings crmUserSettings = _mapper.Map<CrmUserSettings>(request.UserSettings);
            crmUserSettings.AppUserId = request.AppUserId;
            return await _addCrmUserSettingsService.AddCrmUserSettingsAsync(crmUserSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured during adding CRM user settings");
            return false;
        }
    }
}
