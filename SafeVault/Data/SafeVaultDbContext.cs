using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SafeVault.Data;

public class SafeVaultDbContext(DbContextOptions<SafeVaultDbContext> options) : IdentityDbContext(options)
{
}
