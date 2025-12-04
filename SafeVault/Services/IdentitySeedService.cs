using Microsoft.Extensions.Options;
using SafeVault.Config;
using SafeVault.Constants;
using SafeVault.Data;

namespace SafeVault.Services;

/// <summary>
/// Seeds initial users and roles during application startup.
/// </summary>
public class IdentitySeedService(IServiceProvider serviceProvider, IOptions<SeedUserOptions> seedOptions) : BackgroundService
{
    private readonly SeedUserOptions _seeOptions = seedOptions.Value;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await using var db = scope.ServiceProvider.GetRequiredService<SafeVaultDbContext>();

        db.Database.EnsureCreated();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [Roles.Admin, Roles.User];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        var adminUser = await userManager.FindByEmailAsync(_seeOptions.AdminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = Roles.Admin,
                Email = _seeOptions.AdminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, _seeOptions.AdminPassword);
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }

        // Normal user
        var normalUser = await userManager.FindByEmailAsync(_seeOptions.UserEmail);

        if (normalUser == null)
        {
            normalUser = new IdentityUser
            {
                UserName = Roles.User,
                Email = _seeOptions.UserEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(normalUser, _seeOptions.UserPassword);
            await userManager.AddToRoleAsync(normalUser, Roles.User);
        }
    }
}
