using Katino.Domain.Builders;
using Katino.Domain.Context;
using Katino.Domain.Services.AppLogs.GetLogs;
using Katino.Infrastructure.Persistance.Builders;
using Katino.Infrastructure.Persistance.Context;
using Katino.Infrastructure.Persistance.Services.AppLogs;
using Katino.Store.Application.Factories;
using Katino.Store.Application.Services.AuthorizationService;

namespace Katino.Store.Web.Installers;

public class ServiceComponentsDiInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        // contexts
        services.AddScoped<IKatinoDbContext>(sp => sp.GetRequiredService<KatinoDbContext>());

        // builders
        services.AddTransient<IProductQueryBuilder, ProductQueryBuilder>();

        // factories
        services.AddTransient<IAuthTokenFactory, AuthTokenFactory>();

        // services
        services.AddTransient<BaseAuthorizationService, AppUserAuthorizationService>();
        services.AddTransient<IGetLogsService, GetLogsService>();
    }
}
