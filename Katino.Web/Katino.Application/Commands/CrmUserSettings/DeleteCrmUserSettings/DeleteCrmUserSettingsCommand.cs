using MediatR;

namespace Katino.Application.Commands.CrmUserSettingsN.DeleteCrmUserSettings;

public class DeleteCrmUserSettingsCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid AppUserId { get; set; }
}
