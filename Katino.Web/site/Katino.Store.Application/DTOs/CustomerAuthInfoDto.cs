namespace Katino.Store.Application.DTOs;

public class CustomerAuthInfoDto
{
    public Guid CustomerId { get; set; }
    public string Email { get; set; }
    public DateTimeOffset RegistryDate { get; set; }
}
