namespace Katino.Application.DTOs.User;

public class ManageableUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    public DateTime RegistryDate { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
}
