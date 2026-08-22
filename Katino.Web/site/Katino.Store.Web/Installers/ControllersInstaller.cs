using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;

namespace Katino.Store.Web.Installers;

public class ControllersInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });
        services.AddEndpointsApiExplorer();

        // Keeps automatic [Required]/[EmailAddress]-style validation errors in the same
        // ValidationProblemDetails shape and camelCase field-name convention as errors we build
        // manually (see CustomerAuthController.AddModelStateError) - otherwise "Email" (from the
        // C# property name) and "email" (from our own manual errors) would be inconsistent for the frontend.
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                ProblemDetailsFactory problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                ValidationProblemDetails problemDetails = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);

                var camelCasedErrors = problemDetails.Errors
                    .ToDictionary(e => CamelCase(e.Key), e => e.Value);
                problemDetails.Errors.Clear();
                foreach (var (key, value) in camelCasedErrors)
                {
                    problemDetails.Errors[key] = value;
                }

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });
    }

    private static string CamelCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
}
