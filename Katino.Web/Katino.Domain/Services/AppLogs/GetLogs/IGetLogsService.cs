namespace Katino.Domain.Services.AppLogs.GetLogs;

public interface IGetLogsService
{
    Task<(string, string)> GetPlainTextLogs(DateTime date, string directoryName);
    Task<(string, Stream)> GetFileLogs(DateTime date, string directoryName);
}
