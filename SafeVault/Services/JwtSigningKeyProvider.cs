using Microsoft.Extensions.Options;
using SafeVault.Config;

namespace SafeVault.Services;

public class JwtSigningKeyProvider(IOptions<JwtOptions> options) : IJwtSigningKeyProvider
{
    private readonly SymmetricSecurityKey _key = new(
        Convert.FromBase64String(options.Value.Key ?? throw new InvalidOperationException("JWT Key missing in configuration."))
    );

    public SymmetricSecurityKey GetKey() => _key;
}
