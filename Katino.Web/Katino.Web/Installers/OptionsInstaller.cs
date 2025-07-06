using Katino.Domain.Constants;
using Katino.Web.Options;

namespace Katino.Web.Installers;

public class OptionsInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MySqlConfigOptions>(configuration.GetSection(ConfigurationKeys.ConnectionStrings));
    }
}
