using Katino.Domain.Constants;
using Katino.Store.Web.Extensions;
using Swashbuckle.AspNetCore.Swagger;

CreateLogsFolder();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.InstallServices(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
{
    app.UseSwagger(option => option.RouteTemplate = "swagger/{documentName}/swagger.json");
    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint("v1/swagger.json", "Katino.Store.API");
    });
}

//app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void CreateLogsFolder()
{
    var logDir = Environment.GetEnvironmentVariable(EnvVariables.LogsDir);

    if (string.IsNullOrWhiteSpace(logDir))
    {
        if (OperatingSystem.IsWindows())
        {
            logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Katino-Store",
                "logs");
        }
        else
        {
            logDir = "/home/LogFiles/Katino-Store";
        }

        Environment.SetEnvironmentVariable(EnvVariables.LogsDir, logDir);
    }

    Directory.CreateDirectory(logDir);
}