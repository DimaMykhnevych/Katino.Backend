using Katino.Domain.Exceptions;
using Katino.Domain.Models.NovaPost;
using Katino.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Katino.Infrastructure.Persistance.Services.NovaPost;

public class BaseNpApiService
{
    private readonly NovaPostOptions _novaPostOptions;
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public BaseNpApiService(
        HttpClient httpClient,
        IOptions<NovaPostOptions> options,
        ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _novaPostOptions = options.Value;
        _logger = loggerFactory?.CreateLogger(nameof(BaseNpApiService));
    }

    protected async Task<string> GetProcessedStringResponse<T>(NpApiRequest<T> request, JsonSerializerSettings jsonSerializerSettings = null)
    {
        var json = string.Empty;
        if (jsonSerializerSettings == null)
        {
            json = JsonConvert.SerializeObject(request);
        }
        else
        {
            json = JsonConvert.SerializeObject(request, Formatting.Indented, jsonSerializerSettings);
        }

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_novaPostOptions.RequestTimeoutSeconds));

        string responseString;
        try
        {
            var response = await _httpClient.PostAsync(_novaPostOptions.BaseUrl, content, timeoutCts.Token);

            response.EnsureSuccessStatusCode();

            responseString = await response.Content.ReadAsStringAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            _logger.LogError($"NP API request {request.ModelName}.{request.CalledMethod} timed out after {_novaPostOptions.RequestTimeoutSeconds} seconds");
            throw;
        }

        return DecodeUnicodeOnly(responseString);
    }

    protected string DecodeUnicodeOnly(string value)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            value,
            @"\\u([0-9a-fA-F]{4})",
            match => ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString()
        );
    }

    protected void CheckApiResponse<T>(NpApiResponse<T> response)
    {
        if (response == null || !response.Success)
        {
            _logger.LogError($"An error occurred while communicating with NP API: {response.Errors.FirstOrDefault()}");
            throw new NpInternalException();
        }
    }
}
