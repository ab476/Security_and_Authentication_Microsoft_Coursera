using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using SafeVault.Config;
using SafeVault.Data;
using SafeVault.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------------
// Validate DI container at startup
// -----------------------------------------------------------------------------
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// -----------------------------------------------------------------------------
// 1. Default Services (Aspire / Shared Infra)
// -----------------------------------------------------------------------------
builder.AddServiceDefaults();
builder.AddSqlServerClient(connectionName: "safevaultdb");

// -----------------------------------------------------------------------------
// 2. Application Services (DI)
// -----------------------------------------------------------------------------
builder.Services.TryAddScoped<IUserService, UserService>();
builder.Services.TryAddSingleton<ITokenService, TokenService>();
builder.Services.AddHostedService<IdentitySeedService>();
builder.Services.TryAddSingleton<IJwtSigningKeyProvider, JwtSigningKeyProvider>();

// -----------------------------------------------------------------------------
// 3. Database Context
// -----------------------------------------------------------------------------
builder.AddSqlServerDbContext<SafeVaultDbContext>("safevaultdb");

// -----------------------------------------------------------------------------
// 4. Configuration + Options Binding
// -----------------------------------------------------------------------------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SeedUserOptions>(builder.Configuration.GetSection("SeedUsers"));

// -----------------------------------------------------------------------------
// 5. Identity
// -----------------------------------------------------------------------------
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<SafeVaultDbContext>()
.AddDefaultTokenProviders();

// -----------------------------------------------------------------------------
// 6. Authentication (JWT)
// -----------------------------------------------------------------------------
builder.Services.ConfigureOptions<ConfigureJWTBearerOptions>();
builder.Services
    .AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

builder.Services.AddAuthorization();

// -----------------------------------------------------------------------------
// 7. API + Swagger
// -----------------------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options => 
{ 
    options.AddScalarTransformers();
});

builder.Services.AddControllers();

// -----------------------------------------------------------------------------
// 8. Build App
// -----------------------------------------------------------------------------
var app = builder.Build();
app.Use(async (context, next) =>
{
    await next();
});
// -----------------------------------------------------------------------------
// 9. Middleware Pipeline
// -----------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();

app.UseAuthentication();
app.UseAuthorization();

// -----------------------------------------------------------------------------
// 10. Routing
// -----------------------------------------------------------------------------
app.MapControllers();
app.MapUserEndpoints();
app.MapKeyEndpoints();
app.MapGet("/", () => "Hello World!");

// -----------------------------------------------------------------------------
// 11. Run App
// -----------------------------------------------------------------------------
app.Run();
