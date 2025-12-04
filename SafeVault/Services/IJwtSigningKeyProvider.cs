namespace SafeVault.Services;

public interface IJwtSigningKeyProvider
{
    SymmetricSecurityKey GetKey();
}
