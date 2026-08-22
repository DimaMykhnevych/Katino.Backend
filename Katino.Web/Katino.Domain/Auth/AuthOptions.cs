using Katino.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Katino.Domain.Auth;

public class AuthOptions(SecretKeyOptions options)
{
    public const string ISSUER = "Katino.App.API";
    public const string AUDIENCE = "Katino.App.User";
    public const string CUSTOMER_AUDIENCE = "Katino.App.Customer";

    private readonly SecretKeyOptions _secretKeyOptions = options;

    public SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKeyOptions.SecretKey));
    }
}

