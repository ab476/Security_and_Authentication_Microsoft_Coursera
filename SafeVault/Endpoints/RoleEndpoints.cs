using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SafeVault.Endpoints;

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles").WithTags("Roles");

        // Assign role to a user
        group.MapPost("/assign", [Authorize(Roles = "admin")] async Task<Results<Ok, BadRequest<string>>> (
            AssignRoleRequest request,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager) =>
        {
            var user = await userManager.FindByNameAsync(request.Username);
            if (user is null) return TypedResults.BadRequest("User not found.");

            if (!await roleManager.RoleExistsAsync(request.Role))
                await roleManager.CreateAsync(new IdentityRole(request.Role));

            var result = await userManager.AddToRoleAsync(user, request.Role);

            return result.Succeeded
                ? TypedResults.Ok()
                : TypedResults.BadRequest("Failed to assign role.");
        });

        return app;
    }
}

public record AssignRoleRequest(string Username, string Role);
