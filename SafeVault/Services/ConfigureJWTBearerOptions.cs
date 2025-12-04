using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SafeVault.Config;

namespace SafeVault.Services;

public class ConfigureJWTBearerOptions(
    IJwtSigningKeyProvider signingKeyProvider,
    IOptions<JwtOptions> jwtOptionsAccessor
) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtSettings = jwtOptionsAccessor.Value;

    public void Configure(JwtBearerOptions jwtOptions)
    {
        var tvp = jwtOptions.TokenValidationParameters;

        tvp.ValidIssuer = _jwtSettings.Issuer;
        tvp.ValidAudience = _jwtSettings.Audience;
        tvp.IssuerSigningKey = signingKeyProvider.GetKey();

        tvp.ValidateIssuer = true;
        tvp.ValidateAudience = true;
        tvp.ValidateLifetime = true;
        tvp.ValidateIssuerSigningKey = true;
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Console.WriteLine("CustomJwtBearerConfigureOptions ran!");
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            Configure(options);
        }
    }
}
