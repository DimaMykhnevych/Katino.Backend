using AutoMapper;
using Katino.Application.DTOs.NovaPost;
using Katino.Domain.Builders;
using Katino.Domain.Enums.NovaPost;
using Katino.Domain.Services.NovaPost.Sync;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Katino.Application.Queries.NovaPost.GetCurrentSyncStatus;

public class GetCurrentSyncStatusQueryHandler : IRequestHandler<GetCurrentSyncStatusQuery, GetCurrentSyncStatusDto>
{
    private readonly IAppUserQueryBuilder _userQueryBuilder;
    private readonly INovaPoshtaSyncStatusService _novaPoshtaSyncStatusService;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetCurrentSyncStatusQueryHandler(
        IAppUserQueryBuilder userQueryBuilder,
        INovaPoshtaSyncStatusService novaPoshtaSyncStatus,
        IMapper mapper,
        ILoggerFactory loggerFactory)
    {
        _novaPoshtaSyncStatusService = novaPoshtaSyncStatus;
        _userQueryBuilder = userQueryBuilder;
        _mapper = mapper;
        _logger = loggerFactory?.CreateLogger(nameof(GetCurrentSyncStatusQueryHandler));
    }

    public async Task<GetCurrentSyncStatusDto> Handle(GetCurrentSyncStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling get current sync status request");

        var syncType = _mapper.Map<SyncType>(request.SyncType);
        var currentSync = await _novaPoshtaSyncStatusService.GetCurrentSyncStatusAsync(syncType);
        var isInProgress = await _novaPoshtaSyncStatusService.IsSyncInProgressAsync(syncType);

        if (currentSync == null)
        {
            return new GetCurrentSyncStatusDto
            {
                Status = SyncStatusDto.NotStarted,
                IsInProgress = false,
                CanTriggerSync = true,
                TriggeredBy = Guid.Empty,
                TriggeredByUsername = string.Empty
            };
        }

        var statusDto = _mapper.Map<SyncStatusDto>(currentSync.Status);

        var dto = new GetCurrentSyncStatusDto
        {
            Id = currentSync.Id,
            Status = statusDto,
            IsInProgress = isInProgress,
            StartedAt = currentSync.StartedAt,
            CompletedAt = currentSync.CompletedAt,
            ApiRequestedRecords = currentSync.ApiRequestedRecords,
            DbInsertedRecords = currentSync.DbInsertedRecords,
            ErrorMessage = currentSync.ErrorMessage,
            TriggeredBy = currentSync.TriggeredBy,
            CanTriggerSync = !isInProgress,
        };

        if (currentSync.TriggeredBy == Guid.Empty)
        {
            dto.TriggeredByUsername = "System";
        }
        else
        {
            var user = _userQueryBuilder
            .SetBaseUserInfo()
            .SetUserId(currentSync.TriggeredBy)
            .Build()
            .FirstOrDefault();

            dto.TriggeredByUsername = user.UserName;
        }

        if (currentSync.StartedAt.HasValue)
        {
            var endTime = currentSync.CompletedAt ?? DateTimeOffset.UtcNow;
            dto.DurationSeconds = (int)(endTime - currentSync.StartedAt.Value).TotalSeconds;
        }

        return dto;
    }
}
