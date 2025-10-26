using Microsoft.EntityFrameworkCore;
using Katino.Infrastructure.Persistance.Context;
using Katino.Domain.Constants;

namespace Katino.Web.Installers;

public class DbInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration[ConfigurationKeys.DefaultConnectionString];
        services.AddDbContext<KatinoDbContext>(opt =>
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    }
}