using Newtonsoft.Json;

namespace Katino.Web.Installers;

public class ControllersInstaller
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
