namespace SafeVault.Models;

// Input model for creating a user
public record CreateUserRequest(string Username, string Email);

public record GetUserRequest(string Username);

// Response model
public record UserResponse(int Id, string Username, string Email);

