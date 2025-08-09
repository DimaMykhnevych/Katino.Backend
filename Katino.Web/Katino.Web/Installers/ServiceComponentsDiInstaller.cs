using Katino.Application.Factories;
using Katino.Application.Services.AuthorizationService;
using Katino.Domain.Builders;
using Katino.Domain.Context;
using Katino.Domain.Services.AppLogs.GetLogs;
using Katino.Domain.Services.Email.SendEmail;
using Katino.Domain.Services.User.CreateUser;
using Katino.Infrastructure.Persistance.Builders;
using Katino.Infrastructure.Persistance.Context;
using Katino.Infrastructure.Persistance.Services.AppLogs;

namespace Katino.Web.Installers;

public class ServiceComponentsDiInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        // contexts
        services.AddScoped<IKatinoDbContext, KatinoDbContext>();

        // factories
        services.AddTransient<IAuthTokenFactory, AuthTokenFactory>();

        // services
        services.AddTransient<BaseAuthorizationService, AppUserAuthorizationService>();
        services.AddTransient<ISendEmailService, SendEmailService>();
        services.AddTransient<ICreateUserService, CreateUserService>();
        services.AddTransient<IGetLogsService, GetLogsService>();

        // builders
        services.AddTransient<IAppUserQueryBuilder, AppUserQueryBuilder>();

        // repositories
    }
}

