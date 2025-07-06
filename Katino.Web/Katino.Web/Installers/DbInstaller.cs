using Microsoft.EntityFrameworkCore;
using Katino.Infrastructure.Persistance.Context;

namespace Katino.Web.Installers;

public class DbInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration["ConnectionStrings:Default"];
        services.AddDbContext<KatinoDbContext>(opt =>
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    }
}