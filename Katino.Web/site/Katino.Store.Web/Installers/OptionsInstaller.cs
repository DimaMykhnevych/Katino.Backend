using Katino.Domain.Constants;
using Katino.Domain.Options;

namespace Katino.Store.Web.Installers;

public class OptionsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MySqlConfigOptions>(configuration.GetSection(ConfigurationKeys.ConnectionStrings));
        services.Configure<SecretKeyOptions>(configuration.GetSection(ConfigurationKeys.SecretKeyOptions));
        services.Configure<EmailServiceOptions>(configuration.GetSection(ConfigurationKeys.EmailServiceOptions));
        services.Configure<NovaPostOptions>(configuration.GetSection(ConfigurationKeys.NovaPostOptions));
    }
}
