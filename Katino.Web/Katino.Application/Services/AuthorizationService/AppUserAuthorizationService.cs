using Katino.Application.Commands.Auth.SignIn;
using Katino.Application.DTOs;
using Katino.Application.Factories;
using Katino.Domain.Constants;
using Katino.Domain.Entities;
using Katino.Domain.Enums;
using Katino.Domain.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Katino.Application.Services.AuthorizationService;

public class AppUserAuthorizationService : BaseAuthorizationService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AppUserAuthorizationService(
        IAuthTokenFactory tokenFactory,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration configuration)
        : base(tokenFactory)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public override async Task<IEnumerable<Claim>> GetUserClaimsAsync(SignInCommand model)
    {
        AppUser user = await _userManager.FindByNameAsync(model.UserName);

        if (user == null)
        {
            return [];
        }

        return new List<Claim>()
            {
                new(ClaimTypes.Name, user.UserName.ToString()),
                new(AuthorizationConstants.ID, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role)
            };
    }

    public async override Task<LoginErrorCode> VerifyUserAsync(SignInCommand model)
    {
        AppUser user = await _userManager.FindByNameAsync(model.UserName);
        if (user == null)
        {
            return LoginErrorCode.InvalidUsernameOrPassword;
        }

        if (_configuration.EmailConfirmationEnabled() && !await _userManager.IsEmailConfirmedAsync(user))
        {
            return LoginErrorCode.EmailConfirmationRequired;
        }

        SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

        return result.Succeeded ? LoginErrorCode.None : LoginErrorCode.InvalidUsernameOrPassword;
    }

    public async override Task<UserAuthInfoDto> GetUserInfoAsync(string userName)
    {
        if (userName == null) return null;
        AppUser user = await _userManager.FindByNameAsync(userName);

        UserAuthInfoDto info = new()
        {
            Role = user.Role,
            UserId = user.Id,
            UserName = user.UserName,
            RegistryDate = user.RegistryDate,
            Email = user.Email
        };

        return info;
    }
}