using Microsoft.AspNetCore.Mvc;
using SafeVault.Attributes;

namespace SafeVault.Controllers;

[ApiController]
[Route("api/[controller]")]
[AdminAuthorize]
public class AdminController(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager)
    : ControllerBase
{
    /// <summary>
    /// Returns a simple welcome message for admin users.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Dashboard()
    {
        return Ok("Welcome Admin!");
    }

    /// <summary>
    /// Retrieves a list of all registered users.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public IActionResult GetUsers()
    {
        var users = userManager.Users
            .Select(u => new UserDto(u.Id, u.UserName!, u.Email!))
            .ToList();

        return Ok(users);
    }

    /// <summary>
    /// Retrieves all roles assigned to the specified user.
    /// </summary>
    [HttpGet("users/{username}/roles")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoles(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        return Ok(roles);
    }

    /// <summary>
    /// Assigns a role to the specified user. Creates the role if it does not exist.
    /// </summary>
    [HttpPost("users/{username}/roles/{role}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole(string username, string role)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

        var result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
            return BadRequest("Could not assign role.");

        return Ok();
    }

    /// <summary>
    /// Removes a role from the specified user.
    /// </summary>
    [HttpDelete("users/{username}/roles/{role}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveRole(string username, string role)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        var result = await userManager.RemoveFromRoleAsync(user, role);

        if (!result.Succeeded)
            return BadRequest("Could not remove role.");

        return Ok();
    }

    /// <summary>
    /// Deletes the specified user from the system.
    /// </summary>
    [HttpDelete("users/{username}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(string username)
    {
        var user = await userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return BadRequest("Could not delete user.");

        return Ok();
    }

    /// <summary>
    /// Retrieves basic system information including machine name and OS version.
    /// </summary>
    [HttpGet("system-info")]
    [ProducesResponseType(typeof(SystemInfoDto), StatusCodes.Status200OK)]
    public IActionResult SystemInfo()
    {
        var info = new SystemInfoDto(
            MachineName: Environment.MachineName,
            OSVersion: Environment.OSVersion.ToString(),
            Timestamp: DateTime.UtcNow
        );

        return Ok(info);
    }
}
