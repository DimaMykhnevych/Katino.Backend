using Azure.Storage.Blobs;
using Katino.Domain.Constants;

namespace Katino.Web.Installers;

public class BlobServiceClientInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration[ConfigurationKeys.AzureStorageConnectionString];
        services.AddSingleton(new BlobServiceClient(connectionString));
    }
}
