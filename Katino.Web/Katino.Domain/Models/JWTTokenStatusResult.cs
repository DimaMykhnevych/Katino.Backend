using Katino.Domain.Enums;

namespace Katino.Domain.Models;

public class JWTTokenStatusResult
{
    public string Token { get; set; }
    public bool IsAuthorized { get; set; }
    public UserAuthInfo UserInfo { get; set; }
    public LoginErrorCodes LoginErrorCode { get; set; }
}
