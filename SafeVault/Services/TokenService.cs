using Microsoft.Extensions.Options;
using SafeVault.Config;

namespace SafeVault.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwt;

    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenService(IOptions<JwtOptions> jwtOptions, IJwtSigningKeyProvider keyProvider)
    {
        _jwt = jwtOptions.Value;

        var signingKey = keyProvider.GetKey();

        _signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256
        );
        
    }

    public async Task<string> GenerateJwtTokenAsync(IdentityUser user, IList<string> roles)
    {
        // 1️⃣ Claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        };

        // 2️⃣ Roles (async)
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // 3️⃣ Build JWT (fast — everything else precomputed)
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: _signingCredentials
        );

        return _tokenHandler.WriteToken(token);
    }
}
