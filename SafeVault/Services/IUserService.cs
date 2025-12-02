namespace SafeVault.Services;

public interface IUserService
{
    Task<UserResponse?> GetUserAsync(GetUserRequest request, CancellationToken ct);
    Task SaveUserAsync(CreateUserRequest request, CancellationToken ct);
}