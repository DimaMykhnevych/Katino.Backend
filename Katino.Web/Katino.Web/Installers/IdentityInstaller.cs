using Katino.Domain.Auth;
using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Options;
using Katino.Infrastructure.Persistance.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Katino.Web.Installers;


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
            o.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<AppUser>>();

                    var userId = context.Principal.Claims
                        .FirstOrDefault(c => c.Type == AuthorizationConstants.ID)?.Value;

                    if (userId == null)
                    {
                        context.Fail("Unauthorized");
                        return;
                    }

                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null || await userManager.IsLockedOutAsync(user))
                    {
                        context.Fail("Unauthorized");
                    }
                }
            };
        });

        services.AddAuthorization();
    }
}

