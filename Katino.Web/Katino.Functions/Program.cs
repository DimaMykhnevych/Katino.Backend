using Katino.Domain.Constants;
using Katino.Domain.Options;
using Katino.Domain.Repositories.OrderRepository;
using Katino.Domain.Services.NovaPost.InternetDocument;
using Katino.Functions.Services.NpIntDocStatusSyncService;
using Katino.Infrastructure.Persistance.Context;
using Katino.Infrastructure.Persistance.Repositories.OrderRepository;
using Katino.Infrastructure.Persistance.Services.NovaPost;
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

        services.AddHttpClient<IInternetDocumentService, InternetDocumentService>();

        services.AddTransient<INpIntDocStatusSyncService, NpIntDocStatusSyncService>();

        string connectionString = context.Configuration[ConfigurationKeys.DefaultConnectionString];
        services.AddDbContext<KatinoDbContext>(opt =>
                opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddTransient<IOrderRepository, OrderRepository>();
    })
    .Build();

host.Run();