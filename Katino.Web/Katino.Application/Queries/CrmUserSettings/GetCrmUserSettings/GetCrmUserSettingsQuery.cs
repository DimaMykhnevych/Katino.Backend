using Katino.Application.DTOs.CrmUserSettings;
using MediatR;

namespace Katino.Application.Queries.CrmUserSettings.GetCrmUserSettings;

public class GetCrmUserSettingsQuery : IRequest<GetCrmUserSettingsDto>
{
    public Guid AppUserId { get; set; }
}
