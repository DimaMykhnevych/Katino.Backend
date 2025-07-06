using Katino.Application;

namespace Katino.Web.Installers;

public class MediatorInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
                 cfg.RegisterServicesFromAssembly(typeof(AssemblyInfo).Assembly));
    }
}
