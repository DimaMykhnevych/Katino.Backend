using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Katino.Store.Application.Factories;

public interface IAuthTokenFactory
{
    JwtSecurityToken CreateToken(string username, IEnumerable<Claim> businessClaims, TimeSpan? tokenLifetime = null);
    JwtSecurityToken CreateToken(string username, SymmetricSecurityKey secret, string issuer, string audience, IEnumerable<Claim> businessClaims, TimeSpan? tokenLifetime = null);
}
