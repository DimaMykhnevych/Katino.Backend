using Katino.Domain.Enums;

namespace Katino.Store.Application.DTOs;

public class JWTTokenStatusResultDto
{
    public string Token { get; set; }
    public bool IsAuthorized { get; set; }
    public UserAuthInfoDto UserInfo { get; set; }
    public LoginErrorCode LoginErrorCode { get; set; }
}
