using Katino.Application.DTOs.NovaPost;
using MediatR;

namespace Katino.Application.Queries.NovaPost.GetSyncHistory;

public class GetSyncHistoryQuery : IRequest<GetSyncRecordDto>
{
    public int Limit { get; set; }
    public SyncTypeDto SyncType { get; set; }
}
