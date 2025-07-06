using Microsoft.OpenApi.Models;

namespace Katino.Web.Installers;

public class SwaggerInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(x =>
        {
            var security = new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
                };
            x.SwaggerDoc("v1", new OpenApiInfo());
            x.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
            x.AddSecurityRequirement(security);

            x.EnableAnnotations();
        });
    }
}