using Katino.Domain.Enums;

namespace Katino.Store.Application.DTOs;

public class CustomerAuthResultDto
{
    public string Token { get; set; }
    public bool IsAuthorized { get; set; }
    public CustomerAuthInfoDto CustomerInfo { get; set; }
    public LoginErrorCode LoginErrorCode { get; set; }
}
