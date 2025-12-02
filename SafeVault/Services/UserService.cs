using MySqlConnector;

namespace SafeVault.Services;

public class UserService(MySqlDataSource mySqlDataSource) : IUserService
{
    public async Task SaveUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        await using var conn = mySqlDataSource.CreateConnection();
        await conn.OpenAsync(ct);

        const string sql = """
            INSERT INTO Users (Username, Email)
            VALUES (@Username, @Email);
        """;

        await using var cmd = new MySqlCommand(sql, conn);

        cmd.Parameters.Add("@Username", MySqlDbType.VarChar).Value = request.Username;
        cmd.Parameters.Add("@Email", MySqlDbType.VarChar).Value = request.Email;

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<UserResponse?> GetUserAsync(GetUserRequest request, CancellationToken ct)
    {
        await using var conn = mySqlDataSource.CreateConnection();
        await conn.OpenAsync(ct);

        const string sql = """
            SELECT Id, Username, Email
            FROM Users
            WHERE Username = @Username
            LIMIT 1;
        """;

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.Add("@Username", MySqlDbType.VarChar).Value = request.Username;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return new UserResponse(
                Id: reader.GetInt32("Id"),
                Username: reader.GetString("Username"),
                Email: reader.GetString("Email")
            );
        }

        return null;
    }
}
