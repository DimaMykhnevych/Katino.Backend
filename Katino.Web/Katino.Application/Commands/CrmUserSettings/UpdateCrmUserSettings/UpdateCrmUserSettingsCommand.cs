using Katino.Application.DTOs.CrmUserSettings;
using MediatR;

namespace Katino.Application.Commands.CrmUserSettingsN.UpdateCrmUserSettings;

public class UpdateCrmUserSettingsCommand : IRequest<bool>
{
    public Guid AppUserId { get; set; }
    public UpdateCrmUserSettingsDto UserSettings { get; set; }
}
