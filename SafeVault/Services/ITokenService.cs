
namespace SafeVault.Services;

public interface ITokenService
{
    Task<string> GenerateJwtTokenAsync(IdentityUser user, IList<string> roles);
}