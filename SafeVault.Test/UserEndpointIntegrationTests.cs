using SafeVault.Models;
using System.Net;
using System.Net.Http.Json;

namespace SafeVault.Test;

public class UserEndpointsMySqlIntegrationTests(SqlServerTestApplicationFactory factory) :
    IClassFixture<SqlServerTestApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // -----------------------------
    // Valid User Creation Test
    // -----------------------------
    [Fact]
    public async Task CreateUser_Should_Return_201_And_Save_To_MySQL()
    {
        var payload = new CreateUserRequest("john_doe", "john@example.com");

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);
        Assert.Equal("john_doe", created!.Username);

        // GET user should now return 200
        var getUser = await _client.GetAsync("/api/users/john_doe");
        Assert.Equal(HttpStatusCode.OK, getUser.StatusCode);
    }

    // -----------------------------
    // SQL Injection Protection Test
    // -----------------------------
    [Theory]
    [InlineData("' OR 1=1 --")]
    [InlineData("admin'; DROP TABLE Users; --")]
    public async Task CreateUser_Should_Block_SQL_Injection(string malicious)
    {
        var payload = new CreateUserRequest(malicious, "test@example.com");

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // -----------------------------
    // XSS Protection Test
    // -----------------------------
    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    public async Task CreateUser_Should_Block_XSS(string malicious)
    {
        var payload = new CreateUserRequest(malicious, "valid@example.com");

        var response = await _client.PostAsJsonAsync("/api/users", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // -----------------------------
    // GET non-existent user
    // -----------------------------
    [Fact]
    public async Task GetUser_Should_Return_404_If_NotFound()
    {
        var response = await _client.GetAsync("/api/users/does_not_exist");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}