namespace SafeVault.Config;

public class JwtOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }

    /// <summary>
    /// ⏱️ Token expiry in minutes
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;
}
