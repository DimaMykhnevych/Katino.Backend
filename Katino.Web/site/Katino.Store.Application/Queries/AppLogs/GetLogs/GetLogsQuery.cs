using Katino.Domain.Enums;
using Katino.Store.Application.DTOs;
using MediatR;

namespace Katino.Store.Application.Queries.AppLogs.GetLogs;

public class GetLogsQuery : IRequest<LogsDto>
{
    public GetLogsModeDto GetLogsMode { get; set; }
    public DateTime Date { get; set; }
}
