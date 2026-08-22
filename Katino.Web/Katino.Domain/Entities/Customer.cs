namespace Katino.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public string EmailConfirmationToken { get; set; }
    public DateTimeOffset? EmailConfirmationTokenExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
