using Newtonsoft.Json;

namespace Katino.Store.Web.Installers;

public class ControllersInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });
        services.AddEndpointsApiExplorer();
    }
}
