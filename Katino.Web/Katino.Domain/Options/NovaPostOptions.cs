namespace Katino.Domain.Options;

public class NovaPostOptions
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public int InternetDocumentTimeoutSeconds { get; set; } = 30;
    public int RequestTimeoutSeconds { get; set; } = 30;
}
