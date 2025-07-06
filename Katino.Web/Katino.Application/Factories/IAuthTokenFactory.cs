using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Katino.Application.Factories;

public interface IAuthTokenFactory
{
    JwtSecurityToken CreateToken(string username, IEnumerable<Claim> businessClaims);
    JwtSecurityToken CreateToken(string username, SymmetricSecurityKey secret, string issuer, string audience, IEnumerable<Claim> businessClaims);
}
