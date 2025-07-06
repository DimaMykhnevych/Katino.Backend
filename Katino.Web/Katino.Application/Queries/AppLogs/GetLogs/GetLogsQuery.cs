using Katino.Application.DTOs;
using Katino.Domain.Enums;
using MediatR;

namespace Katino.Application.Queries.AppLogs.GetLogs;
public class GetLogsQuery : IRequest<LogsDto>
{
    public GetLogsMode GetLogsMode { get; set; }
    public DateTime Date { get; set; }
}
