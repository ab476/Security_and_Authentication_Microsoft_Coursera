using Microsoft.Data.SqlClient;

namespace SafeVault.Services;

public class UserService(SqlConnection dataSource) : IUserService
{
    public async Task SaveUserAsync(CreateUserRequest request, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO Users (Username, Email)
            VALUES (@Username, @Email);
        """;

        await using var cmd = new SqlCommand(sql, dataSource);

        cmd.Parameters.Add(new SqlParameter("@Username", System.Data.SqlDbType.NVarChar)
        {
            Value = request.Username
        });

        cmd.Parameters.Add(new SqlParameter("@Email", System.Data.SqlDbType.NVarChar)
        {
            Value = request.Email
        });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<UserResponse?> GetUserAsync(GetUserRequest request, CancellationToken ct)
    {
        const string sql = """
            SELECT TOP 1 Id, Username, Email
            FROM Users
            WHERE Username = @Username;
        """;

        await using var cmd = new SqlCommand(sql, dataSource);

        cmd.Parameters.Add(new SqlParameter("@Username", System.Data.SqlDbType.NVarChar)
        {
            Value = request.Username
        });

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (await reader.ReadAsync(ct))
        {
            return new UserResponse(
                Id: reader.GetInt32(reader.GetOrdinal("Id")),
                Username: reader.GetString(reader.GetOrdinal("Username")),
                Email: reader.GetString(reader.GetOrdinal("Email"))
            );
        }

        return null;
    }
}
