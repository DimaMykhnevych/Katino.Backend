namespace Katino.Domain.Services.AppLogs.GetLogs;

public interface IGetLogsService
{
    Task<(string, string)> GetPlainTextLogs(DateTime date);
    Task<(string, Stream)> GetFileLogs(DateTime date);
}
