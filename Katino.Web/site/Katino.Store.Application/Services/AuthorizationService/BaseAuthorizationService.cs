using Katino.Domain.Enums;
using Katino.Store.Application.Commands.Auth.SignIn;
using Katino.Store.Application.DTOs;
using Katino.Store.Application.Factories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Katino.Store.Application.Services.AuthorizationService;

public abstract class BaseAuthorizationService
{
    private readonly IAuthTokenFactory _tokenFactory;
    public BaseAuthorizationService(IAuthTokenFactory tokenFactory)
    {
        _tokenFactory = tokenFactory;
    }
    public async Task<JWTTokenStatusResultDto> GenerateTokenAsync(SignInCommand model)
    {
        LoginErrorCode status = await VerifyUserAsync(model);
        if (status != LoginErrorCode.None)
        {
            return new JWTTokenStatusResultDto()
            {
                Token = null,
                IsAuthorized = false,
                LoginErrorCode = status
            };
        }

        IEnumerable<Claim> claims = await GetUserClaimsAsync(model);
        JwtSecurityToken token = _tokenFactory.CreateToken(model.UserName.ToString(), claims);
        UserAuthInfoDto info = await GetUserInfoAsync(model.UserName);

        return new JWTTokenStatusResultDto()
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            IsAuthorized = true,
            UserInfo = info,
            LoginErrorCode = LoginErrorCode.None
        };
    }

    public abstract Task<IEnumerable<Claim>> GetUserClaimsAsync(SignInCommand model);
    public abstract Task<UserAuthInfoDto> GetUserInfoAsync(string userName);
    public abstract Task<LoginErrorCode> VerifyUserAsync(SignInCommand model);
}
