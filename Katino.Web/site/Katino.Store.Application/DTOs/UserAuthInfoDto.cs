
namespace Katino.Store.Application.DTOs;

public class UserAuthInfoDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    public DateTime RegistryDate { get; set; }
    public string Email { get; set; }
}
