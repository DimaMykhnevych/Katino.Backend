using Azure.Storage.Blobs;
using Katino.Domain.Constants;
using Katino.Domain.Options;
using Katino.Domain.Repositories.FinanceCategoryRepository;
using Katino.Domain.Repositories.FinanceEntryRepository;
using Katino.Domain.Repositories.OrderAddressInfoRepository;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.OrderTagRepository;
using Katino.Domain.Repositories.ProductPhotoRepository;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Repositories.TelegramSettingsRepository;
using Katino.Domain.Services.AzureStorage;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.OrderN.DeleteOrderService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Katino.Domain.Services.TelegramN;
using Katino.Functions.Services.NpIntDocStatusSyncService;
using Katino.Domain.Context;
using Katino.Infrastructure.Persistance.Context;
using Katino.Infrastructure.Persistance.Repositories.FinanceCategoryRepository;
using Katino.Infrastructure.Persistance.Repositories.FinanceEntryRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderAddressInfoRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderItemRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderTagRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductPhotoRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantMeasurementRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantRepository;
using Katino.Infrastructure.Persistance.Repositories.TelegramSettingsRepository;
using Katino.Infrastructure.Persistance.Services.AzureStorage;
using Katino.Infrastructure.Persistance.Services.NovaPost;
using Katino.Infrastructure.Persistance.Services.OrderItemN;
using Katino.Infrastructure.Persistance.Services.OrderN;
using Katino.Infrastructure.Persistance.Services.ProductVariantN;
using Katino.Infrastructure.Persistance.Services.TelegramN;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.ApplicationInsights.Extensibility;
using Katino.Domain.Services.NovaPost.ContactPerson;
using Katino.Domain.Repositories.OrderRecipientRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderRecipientRepository;
using Katino.Domain.Services.OrderRecipientN.AddOrderRecipientService;
using Katino.Infrastructure.Persistance.Services.OrderRecipientN;
using Katino.Domain.Services.NpOptionsSeatN.AddNpOptionsSeatService;
using Katino.Infrastructure.Persistance.Services.NpOptionsSeatN;
using Katino.Domain.Repositories.NpOptionsSeatRepository;
using Katino.Infrastructure.Persistance.Repositories.NpOptionsSeatRepository;
using Katino.Domain.Services.OrderN.OrderDeliveryHandler;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

        config.AddEnvironmentVariables();

        if (context.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<Program>();
        }
    })
    .UseSerilog((context, services, loggerConfig) =>
    {
        var telemetryConfig = services.GetRequiredService<TelemetryConfiguration>();

        loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(telemetryConfig, TelemetryConverter.Traces);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();

        services.Configure<NovaPostOptions>(context.Configuration.GetSection(ConfigurationKeys.NovaPostOptions));

        // HTTP clients
        services.AddHttpClient<IInternetDocumentService, InternetDocumentService>();
        services.AddHttpClient<IContactPersonService, ContactPersonService>();

        // Services
        services.AddTransient<INpIntDocStatusSyncService, NpIntDocStatusSyncService>();
        services.AddTransient<IOrderItemChangeService, OrderItemChangeService>();
        services.AddTransient<IUpdateProductVariantService, UpdateProductVariantService>();
        services.AddTransient<IAzureStorageService, AzureStorageService>();
        services.AddTransient<IDeleteOrderService, DeleteOrderService>();

        services.AddTransient<NovaPostDeliveryHandler>();
        services.AddTransient<NonNovaPostDeliveryHandler>();
        services.AddTransient<IOrderDeliveryHandlerFactory, OrderDeliveryHandlerFactory>();
        services.AddTransient<IAddOrderRecipientService, AddOrderRecipientService>();
        services.AddTransient<IAddNpOptionsSeatService, AddNpOptionsSeatService>();
        services.AddTransient<ITelegramService, TelegramService>();
        services.AddTransient<IOrderRejectionNotifier, OrderRejectionNotifier>();

        string connectionString = context.Configuration[ConfigurationKeys.DefaultConnectionString];
        services.AddDbContext<KatinoDbContext>(opt =>
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        services.AddScoped<IKatinoDbContext>(sp => sp.GetRequiredService<KatinoDbContext>());

        var azureStorageConnectionString = context.Configuration[ConfigurationKeys.AzureStorageConnectionString];
        services.AddSingleton(new BlobServiceClient(azureStorageConnectionString));

        // Repos
        services.AddTransient<IOrderRepository, OrderRepository>();
        services.AddTransient<IOrderTagRepository, OrderTagRepository>();
        services.AddTransient<IProductVariantRepository, ProductVariantRepository>();
        services.AddTransient<IOrderItemRepository, OrderItemRepository>();
        services.AddTransient<IProductVariantMeasurementRepository, ProductVariantMeasurementRepository>();
        services.AddTransient<IProductPhotoRepository, ProductPhotoRepository>();
        services.AddTransient<IOrderAddressInfoRepository, OrderAddressInfoRepository>();
        services.AddTransient<IFinanceEntryRepository, FinanceEntryRepository>();
        services.AddTransient<IFinanceCategoryRepository, FinanceCategoryRepository>();
        services.AddTransient<IOrderRecipientRepository, OrderRecipientRepository>();
        services.AddTransient<INpOptionsSeatRepository, NpOptionsSeatRepository>();
        services.AddTransient<ITelegramSettingsRepository, TelegramSettingsRepository>();
    })
    .Build();

host.Run();