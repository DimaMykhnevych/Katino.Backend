using Microsoft.AspNetCore.Identity;

namespace Katino.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string Role { get; set; }
    public DateTime RegistryDate { get; set; }
}
