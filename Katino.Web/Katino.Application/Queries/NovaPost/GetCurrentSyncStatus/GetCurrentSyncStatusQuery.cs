using Katino.Application.DTOs.NovaPost;
using MediatR;

namespace Katino.Application.Queries.NovaPost.GetCurrentSyncStatus;

public class GetCurrentSyncStatusQuery : IRequest<GetCurrentSyncStatusDto>
{
    public SyncTypeDto SyncType { get; set; }
}
