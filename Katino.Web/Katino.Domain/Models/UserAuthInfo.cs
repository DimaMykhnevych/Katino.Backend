namespace Katino.Domain.Models;

public class UserAuthInfo
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    public DateTime RegistryDate { get; set; }
    public string Email { get; set; }
}

