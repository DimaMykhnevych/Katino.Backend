using Katino.Application.DTOs.CrmUserSettings;
using MediatR;

namespace Katino.Application.Commands.CrmUserSettingsN.AddCrmUserSettings;

public class AddCrmUserSettingsCommand : IRequest<bool>
{
    public Guid AppUserId { get; set; }
    public AddCrmUserSettingsDto UserSettings { get; set; }
}
