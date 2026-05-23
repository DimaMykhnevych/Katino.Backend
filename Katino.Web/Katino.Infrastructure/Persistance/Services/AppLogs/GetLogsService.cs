using Katino.Domain.Constants;
using Katino.Domain.Services.AppLogs.GetLogs;
using Microsoft.Extensions.Logging;

namespace Katino.Infrastructure.Persistance.Services.AppLogs;

public class GetLogsService : IGetLogsService
{
    private readonly ILogger _logger;

    public GetLogsService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory?.CreateLogger(nameof(GetLogsService));
    }

    public async Task<(string, Stream)> GetFileLogs(DateTime date, string directoryName)
    {
        _logger.LogDebug("Getting file logs by {date}", date.ToString("yyyy-MM-dd"));
        var (fileName, logs) = await GetPlainTextLogsInternal(date, directoryName);
        return (fileName, GenerateStreamFromString(logs));
    }

    public async Task<(string, string)> GetPlainTextLogs(DateTime date, string directoryName)
    {
        _logger.LogDebug("Getting plain text logs by {date}", date.ToString("yyyy-MM-dd"));
        return await GetPlainTextLogsInternal(date, directoryName);
    }

    private async Task<(string, string)> GetPlainTextLogsInternal(DateTime date, string directoryName)
    {
        var fullLogsPath = GetLogsPath(directoryName);
        if (!Directory.Exists(fullLogsPath))
        {
            Directory.CreateDirectory(fullLogsPath);
        }

        if (date == DateTime.MinValue)
        {
            date = DateTime.UtcNow;
        }

        string[] fileEntries = Directory.GetFiles(fullLogsPath);
        string pattern = $"{date:yyyyMMdd}";
        string neededDateLogsFilePath = fileEntries.FirstOrDefault(f => f.Contains(pattern));
        if (string.IsNullOrEmpty(neededDateLogsFilePath))
        {
            _logger.LogWarning("Logs by {date} weren't found", date.ToString("yyyy-MM-dd"));
            return (Path.GetFileName(neededDateLogsFilePath), string.Empty);
        }

        string content = string.Empty;
        using var fs = new FileStream(neededDateLogsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        content += await sr.ReadToEndAsync();
        return (Path.GetFileName(neededDateLogsFilePath), content);
    }

    private static string GetLogsPath(string directoryName)
    {
        var logsDirFromEnv = Environment.GetEnvironmentVariable(EnvVariables.LogsDir);
        if (!string.IsNullOrEmpty(logsDirFromEnv))
        {
            return logsDirFromEnv;
        }

        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                directoryName,
                "logs");
        }
        else
        {
            return $"/home/LogFiles/{directoryName}";
        }
    }

    private static Stream GenerateStreamFromString(string s)
    {
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}