using Azure.Storage.Blobs;
using Katino.Domain.Constants;
using Katino.Domain.Options;
using Katino.Domain.Repositories.OrderItemRepository;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Repositories.ProductPhotoRepository;
using Katino.Domain.Repositories.ProductVariantMeasurementRepository;
using Katino.Domain.Repositories.ProductVariantRepository;
using Katino.Domain.Services.AzureStorage;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Domain.Services.OrderItemN.OrderItemChangeService;
using Katino.Domain.Services.ProductVariantN.UpdateProductVariantService;
using Katino.Functions.Services.NpIntDocStatusSyncService;
using Katino.Infrastructure.Persistance.Context;
using Katino.Infrastructure.Persistance.Repositories.OrderItemRepository;
using Katino.Infrastructure.Persistance.Repositories.OrderRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductPhotoRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantMeasurementRepository;
using Katino.Infrastructure.Persistance.Repositories.ProductVariantRepository;
using Katino.Infrastructure.Persistance.Services.AzureStorage;
using Katino.Infrastructure.Persistance.Services.NovaPost;
using Katino.Infrastructure.Persistance.Services.OrderItemN;
using Katino.Infrastructure.Persistance.Services.ProductVariantN;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.Configure<NovaPostOptions>(context.Configuration.GetSection(ConfigurationKeys.NovaPostOptions));

        // HTTP clients
        services.AddHttpClient<IInternetDocumentService, InternetDocumentService>();

        // Services
        services.AddTransient<INpIntDocStatusSyncService, NpIntDocStatusSyncService>();
        services.AddTransient<IOrderItemChangeService, OrderItemChangeService>();
        services.AddTransient<IUpdateProductVariantService, UpdateProductVariantService>();
        services.AddTransient<IAzureStorageService, AzureStorageService>();

        string connectionString = context.Configuration[ConfigurationKeys.DefaultConnectionString];
        services.AddDbContext<KatinoDbContext>(opt =>
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        var azureStorageConnectionString = context.Configuration[ConfigurationKeys.AzureStorageConnectionString];
        services.AddSingleton(new BlobServiceClient(azureStorageConnectionString));

        // Repos
        services.AddTransient<IOrderRepository, OrderRepository>();
        services.AddTransient<IProductVariantRepository, ProductVariantRepository>();
        services.AddTransient<IOrderItemRepository, OrderItemRepository>();
        services.AddTransient<IProductVariantMeasurementRepository, ProductVariantMeasurementRepository>();
        services.AddTransient<IProductPhotoRepository, ProductPhotoRepository>();
    })
    .Build();

host.Run();