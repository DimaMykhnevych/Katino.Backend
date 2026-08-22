namespace Katino.Domain.Constants;

public class Role
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Sewer = "Sewer";
    public const string DirectManager = "DirectManager";
    public const string Customer = "Customer";

    public static bool IsAdminRole(string role)
    {
        return role == Admin || role == Owner;
    }
}

