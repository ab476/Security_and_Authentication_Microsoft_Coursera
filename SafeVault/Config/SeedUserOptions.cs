namespace SafeVault.Config;

public class SeedUserOptions
{
    public string AdminEmail { get; set; } = "admin@safevault.com";
    public string AdminPassword { get; set; } = "Admin@12345";

    public string UserEmail { get; set; } = "user@safevault.com";
    public string UserPassword { get; set; } = "User@12345";
}
