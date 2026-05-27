using Katino.Domain.Auth;
using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Options;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Katino.Store.Web.Installers;

public class IdentityInstaller : IInstaller
{
    public void InstallServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<AppUser, UserRole>()
            .AddEntityFrameworkStores<KatinoDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(o =>
        {
            o.IncludeErrorDetails = true;

            var secOpts = configuration
                .GetSection(ConfigurationKeys.SecretKeyOptions)
                .Get<SecretKeyOptions>();
            var options = new AuthOptions(secOpts);
            o.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateActor = false,
                ValidIssuer = AuthOptions.ISSUER,
                ValidAudience = AuthOptions.AUDIENCE,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = options.GetSymmetricSecurityKey(),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };
        });

        services.AddAuthorization();
    }
}