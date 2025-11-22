using AutoMapper;
using Katino.Application.DTOs.NovaPost;
using Katino.Domain.Builders;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Services.NovaPost.Sync;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.NovaPost.GetSyncHistory;

public class GetSyncHistoryQueryHandler : IRequestHandler<GetSyncHistoryQuery, GetSyncRecordDto>
{
    private readonly INovaPoshtaSyncStatusService _novaPoshtaSyncStatusService;
    private readonly IAppUserQueryBuilder _userQueryBuilder;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetSyncHistoryQueryHandler(
        INovaPoshtaSyncStatusService novaPoshtaSyncStatus,
        IAppUserQueryBuilder userQueryBuilder,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _novaPoshtaSyncStatusService = novaPoshtaSyncStatus;
        _userQueryBuilder = userQueryBuilder;
        _logger = loggerFactory?.CreateLogger(nameof(GetSyncHistoryQueryHandler));
        _mapper = mapper;
    }

    public async Task<GetSyncRecordDto> Handle(GetSyncHistoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get sync history");
        ArgumentNullException.ThrowIfNull(request);

        var syncType = _mapper.Map<SyncType>(request.SyncType);
        var history = await _novaPoshtaSyncStatusService.GetSyncHistoryAsync(syncType, request.Limit);

        var mappedHistoryItems = _mapper.Map<IEnumerable<SyncRecordDto>>(history);
        foreach (var mappedHistoryItem in mappedHistoryItems)
        {
            if (mappedHistoryItem.TriggeredBy == Guid.Empty)
            {
                mappedHistoryItem.TriggeredByUsername = "System";
            }
            else
            {
                var user = _userQueryBuilder
                .SetBaseUserInfo()
                .SetUserId(mappedHistoryItem.TriggeredBy)
                .Build()
                .FirstOrDefault();

                mappedHistoryItem.TriggeredByUsername = user.UserName;
            }
        }

        return new GetSyncRecordDto() { SyncRecords = mappedHistoryItems, ResultsAmount = mappedHistoryItems.Count() };
    }
}
