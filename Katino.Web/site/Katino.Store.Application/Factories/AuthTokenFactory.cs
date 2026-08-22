using Katino.Domain.Auth;
using Katino.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Katino.Store.Application.Factories;

public class AuthTokenFactory(IOptions<SecretKeyOptions> secretKeyOptions) : IAuthTokenFactory
{
    private static readonly TimeSpan DefaultTokenLifetime = TimeSpan.FromDays(30);

    private readonly SecretKeyOptions _secretKeyOptions = secretKeyOptions.Value;

    public JwtSecurityToken CreateToken(string username, IEnumerable<Claim> businessClaims, TimeSpan? tokenLifetime = null)
    {
        var authOptions = new AuthOptions(_secretKeyOptions);
        return CreateToken(username, authOptions.GetSymmetricSecurityKey(), AuthOptions.ISSUER, AuthOptions.AUDIENCE, businessClaims, tokenLifetime);
    }

    public JwtSecurityToken CreateToken(string username, SymmetricSecurityKey secret, string issuer, string audience, IEnumerable<Claim> businessClaims, TimeSpan? tokenLifetime = null)
    {
        List<Claim> claims =
            [
                new Claim(JwtRegisteredClaimNames.Sub,username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                .. businessClaims,
            ];

        SigningCredentials signinCredentials = new(secret, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwtSecurityToken = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(tokenLifetime ?? DefaultTokenLifetime),
            signingCredentials: signinCredentials);

        return jwtSecurityToken;
    }
}
