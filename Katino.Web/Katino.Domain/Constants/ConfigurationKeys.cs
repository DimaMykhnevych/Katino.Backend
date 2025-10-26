namespace Katino.Domain.Constants;

public static class ConfigurationKeys
{
    public const string ApplicationInsightsConnectionString = "APPLICATIONINSIGHTS_CONNECTION_STRING";
    public const string EmailConfirmationEnabled = "EmailConfirmationEnabled";
    public const string DefaultConnectionString = "ConnectionStrings:Default";
    public const string AzureStorageConnectionString = "ConnectionStrings:AzureStorage";
    public const string StoragePhotoContainerName = "AzureStorage:PhotoContainerName";
    public const string ConnectionStrings = "ConnectionStrings";
    public const string SecretKeyOptions = "SecretKeyOptions";
    public const string EmailServiceOptions = "EmailServiceOptions";
}
