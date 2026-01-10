using Katino.Application.Factories;
using Katino.Application.Services.AuthorizationService;
using Katino.Domain.Builders;
using Katino.Domain.Context;
using Katino.Domain.Repositories.CategoryRepository;
using Katino.Domain.Repositories.ColorRepository;
using Katino.Domain.Repositories.CrmUserSettingsRepository;
using Katino.Domain.Repositories.MeasurementTypeRepository;
using Katino.Domain.Repositories.NovaPoshtaSyncStatusRepository;
using Katino.Domain.Repositories.NpCityRepository;
using Katino.Domain.Repositories.NpContactPersonRepository;
using Katino.Domain.Repositories.NpOptionsSeatRepository;
using Katino.Domain.Repositories.NpWarehouseRepository;
using Katino.Domain.Repositories.OrderAddressInfoRepository;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.OrderNpOptionsSeatRepository;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductPhotoRepository;
using Katino.Domain.Repositories.ProductRepository;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Repositories.SewingHistoryRepository;
using Katino.Domain.Repositories.SizeRepository;
using Katino.Domain.Services.AppLogs.GetLogs;
using Katino.Domain.Services.Article.GenerateArticle;
using Katino.Domain.Services.AzureStorage;
using Katino.Domain.Services.CrmUserSettingsN.AddCrmUserSettingsService;
using Katino.Domain.Services.CrmUserSettingsN.UpdateCrmUserSettingsService;
using Katino.Domain.Services.Email.SendEmail;
using Katino.Domain.Services.NovaPost.City;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.NovaPost.Sync;
using Katino.Domain.Services.NovaPost.Warehouse;
using Katino.Domain.Services.NpCityN.AddNpCityService;
using Katino.Domain.Services.NpContactPersonN.AddNpContactPersonService;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderItemN.SewingProductionReportService;
using Katino.Domain.Services.OrderItemN.SewingQueueService;
using Katino.Domain.Services.OrderN.AddOrderService;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using Katino.Domain.Services.OrderN.SetOrderManualStatusService;
using Katino.Domain.Services.OrderN.UpdateOrderService;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Katino.Domain.Services.User.CreateUser;
using Katino.Infrastructure.Persistance.Builders;
using Katino.Infrastructure.Persistance.Context;
using Katino.Infrastructure.Persistance.Repositories.CategoryRepository;
using Katino.Infrastructure.Persistance.Repositories.ColorRepository;
using Katino.Infrastructure.Persistance.Repositories.CrmUserSettingsRepository;
using Katino.Infrastructure.Persistance.Repositories.MeasurementTypeRepository;
using Katino.Infrastructure.Persistance.Repositories.NovaPoshtaSyncStatusRepository;
using Katino.Infrastructure.Persistance.Repositories.NpCityRepository;
using Katino.Infrastructure.Persistance.Repositories.NpContactPersonRepository;
using Katino.Infrastructure.Persistance.Repositories.NpOptionsSeatRepository;
using Katino.Infrastructure.Persistance.Repositories.NpWarehouseRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderAddressInfoRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderItemRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderNpOptionsSeatRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderRecipientRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductPhotoRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantMeasurementRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantRepository;
using Katino.Infrastructure.Persistance.Repositories.SewingHistoryRepository;
using Katino.Infrastructure.Persistance.Repositories.SizeRepository;
using Katino.Infrastructure.Persistance.Services.AppLogs;
using Katino.Infrastructure.Persistance.Services.Article;
using Katino.Infrastructure.Persistance.Services.AzureStorage;
using Katino.Infrastructure.Persistance.Services.CrmUserSettingsN;
using Katino.Infrastructure.Persistance.Services.NovaPost;
using Katino.Infrastructure.Persistance.Services.NpCityN;
using Katino.Infrastructure.Persistance.Services.NpContactPersonN;
using Katino.Infrastructure.Persistance.Services.NpOptionsSeatN;
using Katino.Infrastructure.Persistance.Services.OrderItemN;
using Katino.Infrastructure.Persistance.Services.OrderN;
using Katino.Infrastructure.Persistance.Services.OrderRecipientN;
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
        services.AddTransient<IArticleGenerator, ArticleGenerator>();
        services.AddTransient<IAzureStorageService, AzureStorageService>();
        services.AddTransient<IAddOrderService, AddOrderService>();
        services.AddTransient<IUpdateOrderService, UpdateOrderService>();
        services.AddTransient<IDeleteOrderService, DeleteOrderService>();
        services.AddTransient<INovaPoshtaSyncService, NovaPoshtaSyncService>();
        services.AddTransient<INovaPoshtaSyncStatusService, NovaPoshtaSyncStatusService>();
        services.AddTransient<IAddNpCityService, AddNpCityService>();
        services.AddTransient<IAddCrmUserSettingsService, AddCrmUserSettingsService>();
        services.AddTransient<IUpdateCrmUserSettingsService, UpdateCrmUserSettingsService>();
        services.AddTransient<IAddNpContactPersonService, AddNpContactPersonService>();
        services.AddTransient<IAddOrderRecipientService, AddOrderRecipientService>();
        services.AddTransient<IAddNpOptionsSeatService, AddNpOptionsSeatService>();
        services.AddTransient<IOrderItemChangeService, OrderItemChangeService>();
        services.AddTransient<ISetOrderManualStatusService, SetOrderManualStatusService>();
        services.AddTransient<ISewingQueueService, SewingQueueService>();
        services.AddTransient<ISewingProductionReportService, SewingProductionReportService>();

        // hosted services
        services.AddHostedService<NovaPoshtaSyncBackgroundService>();

        // HTTP clients
        services.AddHttpClient<IInternetDocumentService, InternetDocumentService>();
        services.AddHttpClient<IWarehouseService, WarehouseService>();
        services.AddHttpClient<INpCityService, NpCityService>();
        services.AddHttpClient<IContactPersonService, ContactPersonService>();

        // builders
        services.AddTransient<IAppUserQueryBuilder, AppUserQueryBuilder>();
        services.AddTransient<IOrderQueryBuilder, OrderQueryBuilder>();

        // repositories
        services.AddTransient<IProductRepository, ProductRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();
        services.AddTransient<ISizeRepository, SizeRepository>();
        services.AddTransient<IColorRepository, ColorRepository>();
        services.AddTransient<IMeasurementTypeRepository, MeasurementTypeRepository>();
        services.AddTransient<IProductVariantRepository, ProductVariantRepository>();
        services.AddTransient<IProductVariantMeasurementRepository, ProductVariantMeasurementRepository>();
        services.AddTransient<IProductPhotoRepository, ProductPhotoRepository>();
        services.AddTransient<INpWarehouseRepository, NpWarehouseRepository>();
        services.AddTransient<INovaPoshtaSyncStatusRepository, NovaPoshtaSyncStatusRepository>();
        services.AddTransient<INpCityRepository, NpCityRepository>();
        services.AddTransient<ICrmUserSettingsRepository, CrmUserSettingsRepository>();
        services.AddTransient<INpContactPersonRepository, NpContactPersonRepository>();
        services.AddTransient<IOrderRecipientRepository, OrderRecipientRepository>();
        services.AddTransient<INpOptionsSeatRepository, NpOptionsSeatRepository>();
        services.AddTransient<IOrderRepository, OrderRepository>();
        services.AddTransient<IOrderItemRepository, OrderItemRepository>();
        services.AddTransient<IOrderAddressInfoRepository, OrderAddressInfoRepository>();
        services.AddTransient<IOrderNpOptionsSeatRepository, OrderNpOptionsSeatRepository>();
        services.AddTransient<ISewingHistoryRepository, SewingHistoryRepository>();
    }
}

