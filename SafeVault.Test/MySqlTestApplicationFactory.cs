using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Testcontainers.MySql;

namespace SafeVault.Test;

public class MySqlTestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer _mysqlContainer;

    public string ConnectionString => _mysqlContainer.GetConnectionString();

    public MySqlTestApplicationFactory()
    {
        _mysqlContainer = new MySqlBuilder()
            .WithDatabase("safevault_test")
            .WithUsername("root")
            .WithPassword("password123")
            .WithReuse(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _mysqlContainer.StartAsync();

        // Apply schema
        using var conn = new MySqlConnection(ConnectionString);
        await conn.OpenAsync();

        const string sql = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(100) NOT NULL UNIQUE,
                Email VARCHAR(200) NOT NULL
            );
        """;
        await using var createCmd = new MySqlCommand(sql, conn);
        await createCmd.ExecuteNonQueryAsync();

        const string truncateSql = "TRUNCATE TABLE Users;";

        await using var truncateCmd = new MySqlCommand(truncateSql, conn);
        await truncateCmd.ExecuteNonQueryAsync();
    }

    public new async Task DisposeAsync()
    {
        await _mysqlContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production MySqlDataSource
            var descriptor = services.SingleOrDefault(
                x => x.ServiceType == typeof(MySqlDataSource));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add test MySqlDataSource pointing to the container
            services.AddSingleton(provider =>
            {
                var dataSourceBuilder = new MySqlDataSourceBuilder(ConnectionString);
                return dataSourceBuilder.Build();
            });
        });
    }
}