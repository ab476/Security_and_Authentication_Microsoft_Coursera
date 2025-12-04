using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace SafeVault.Test;

public class SqlServerTestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer;

    public string ConnectionString => _sqlContainer.GetConnectionString();

    public SqlServerTestApplicationFactory()
    {
        _sqlContainer = new MsSqlBuilder()
            .WithPassword("Password123!")
            .WithReuse(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string createSql = """
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
            CREATE TABLE Users (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Username NVARCHAR(100) NOT NULL UNIQUE,
                Email NVARCHAR(200) NOT NULL
            );
        """;

        await using var createCmd = new SqlCommand(createSql, conn);
        await createCmd.ExecuteNonQueryAsync();

        const string truncateSql = "TRUNCATE TABLE Users;";

        await using var truncateCmd = new SqlCommand(truncateSql, conn);
        await truncateCmd.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production registered SqlConnection or DataSource
            var oldDescriptor = services.SingleOrDefault(x =>
                x.ServiceType == typeof(string) &&
                x.ImplementationInstance is string cs &&
                cs.Contains("Server=", StringComparison.OrdinalIgnoreCase)
            );

            if (oldDescriptor != null)
                services.Remove(oldDescriptor);

            // Add test connection string for SQL Server container
            services.AddSingleton<string>(ConnectionString);
        });
    }
}
