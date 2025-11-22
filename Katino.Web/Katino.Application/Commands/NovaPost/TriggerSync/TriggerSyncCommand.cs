using Katino.Application.DTOs.NovaPost;
using MediatR;

namespace Katino.Application.Commands.NovaPost.TriggerSync;

public class TriggerSyncCommand : IRequest<bool>
{
    public SyncTypeDto SyncType { get; set; }
    public Guid TriggeredBy { get; set; }
}
