using Katino.Application.Factories;
using Katino.Application.Services.AuthorizationService;
using Katino.Domain.Builders;
using Katino.Domain.Context;
using Katino.Domain.Repositories.CategoryRepository;
using Katino.Domain.Repositories.ColorRepository;
using Katino.Domain.Repositories.MeasurementTypeRepository;
using Katino.Domain.Repositories.ProductRepository;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Repositories.SizeRepository;
using Katino.Domain.Services.AppLogs.GetLogs;
using Katino.Domain.Services.Email.SendEmail;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Katino.Domain.Services.User.CreateUser;
using Katino.Infrastructure.Persistance.Builders;
using Katino.Infrastructure.Persistance.Context;
using Katino.Infrastructure.Persistance.Repositories.CategoryRepository;
using Katino.Infrastructure.Persistance.Repositories.ColorRepository;
using Katino.Infrastructure.Persistance.Repositories.MeasurementTypeRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantMeasurementRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantRepository;
using Katino.Infrastructure.Persistance.Repositories.SizeRepository;
using Katino.Infrastructure.Persistance.Services.AppLogs;
using Katino.Infrastructure.Persistance.Services.ProductVariantN;

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
        services.AddTransient<IUpdateProductVariantService, UpdateProductVariantService>();

        // builders
        services.AddTransient<IAppUserQueryBuilder, AppUserQueryBuilder>();

        // repositories
        services.AddTransient<IProductRepository, ProductRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();
        services.AddTransient<ISizeRepository, SizeRepository>();
        services.AddTransient<IColorRepository, ColorRepository>();
        services.AddTransient<IMeasurementTypeRepository, MeasurementTypeRepository>();
        services.AddTransient<IProductVariantRepository, ProductVariantRepository>();
        services.AddTransient<IProductVariantMeasurementRepository, ProductVariantMeasurementRepository>();
    }
}

