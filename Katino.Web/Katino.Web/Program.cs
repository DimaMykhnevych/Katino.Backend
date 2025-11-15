using Katino.Web.Extensions;
using Katino.Web.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.InstallServices(builder.Configuration);
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
SwaggerOptions swaggerOptions = new();
builder.Configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(option => option.RouteTemplate = swaggerOptions.JsonRoute);
    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint(swaggerOptions.UiEndpoint, swaggerOptions.Description);
    });
}

// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
